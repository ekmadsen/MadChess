// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2026.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System;
using ErikTheCoder.MadChess.Core;
using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Core.Utilities;
using ErikTheCoder.MadChess.Engine.Score;
#pragma warning disable IDE0047


namespace ErikTheCoder.MadChess.Engine.Intelligence;


public sealed class TimeManagement(Messenger messenger)
{
    public readonly TimeSpan?[] TimeRemaining = new TimeSpan?[2]; // [color]
    public readonly TimeSpan?[] TimeIncrement = new TimeSpan?[2]; // [color]
    public int? MovesToTimeControl;
    public int HorizonLimit;
    public long NodeLimit;
    public int? MateInMoves;
    public TimeSpan MoveTimeSoftLimit;
    public TimeSpan MoveTimeHardLimit;
    public bool CanIncreaseMoveTime;

    private const int _moveTimePer1024 = 42;
    private const int _incrementTimePer128 = 96;
    private const int _movesRemainingTimePressure = 4;
    private const int _hardLimitPer128 = 384;
    private const int _timeBankPer128 = 256;
    private const int _minToHorizon = 17;
    private const int _smallScoreDecrease = 33;
    private const int _smallScoreDecreaseTimePer128 = 128;
    private const int _largeScoreDecrease = 100;
    private const int _largeScoreDecreaseTimePer128 = 256;
    private const int _haveTimeSearchNextPlyPer128 = 70;

    private readonly TimeSpan _moveTimeReserved = TimeSpan.FromMilliseconds(100);

    private int _initialSoftLimitMilliseconds;
    private int _timeBankMilliseconds;


    public void DetermineMoveTime(Position position, TimeSpan searchTimeElapsed)
    {
        // No need to calculate move time if go command specifies move time, horizon limit, or nodes.
        if ((MoveTimeHardLimit != TimeSpan.MaxValue) || (HorizonLimit != Search.MaxHorizon) || (NodeLimit != long.MaxValue)) return;

        // Retrieve time remaining and time increment.
        var timeRemaining = TimeRemaining[(int)position.ColorToMove] ?? throw new Exception($"{nameof(TimeRemaining)} for {position.ColorToMove} is null.");
        if (timeRemaining == TimeSpan.MaxValue) return; // No need to calculate move time if go command specifies infinite search.
        timeRemaining -= searchTimeElapsed + _moveTimeReserved;
        var timeIncrement = TimeIncrement[(int)position.ColorToMove] ?? TimeSpan.Zero;

        // Calculate move time.
        double milliseconds;
        if (MovesToTimeControl.HasValue)
        {
            // Specific number of moves must be made before time expires.
            var millisecondsRemaining = timeRemaining.TotalMilliseconds + (MovesToTimeControl.Value * timeIncrement.TotalMilliseconds);
            milliseconds = millisecondsRemaining / MovesToTimeControl.Value;
        }
        else
        {
            // Game must be completed before time expires.
            milliseconds = (timeRemaining.TotalMilliseconds * _moveTimePer1024) / 1024;
            milliseconds += (timeIncrement.TotalMilliseconds * _incrementTimePer128) / 128;
        }

        MoveTimeSoftLimit = TimeSpan.FromMilliseconds(milliseconds);
        MoveTimeHardLimit = TimeSpan.FromMilliseconds((milliseconds * _hardLimitPer128) / 128);
        _initialSoftLimitMilliseconds = (int)milliseconds;
        _timeBankMilliseconds = (int)(milliseconds * _timeBankPer128) / 128;

        if (MoveTimeHardLimit > timeRemaining) PreventLossOnTime(timeRemaining, timeIncrement);

        if (messenger.Debug)
        {
            messenger.WriteLine($"info string TimeRemaining = {timeRemaining.TotalMilliseconds:0} ms TimeIncrement = {timeIncrement.TotalMilliseconds:0} ms MovesToTimeControl = {MovesToTimeControl}");
            messenger.WriteLine($"info string MoveTimeSoftLimit = {MoveTimeSoftLimit.TotalMilliseconds:0} ms MoveTimeHardLimit = {MoveTimeHardLimit.TotalMilliseconds:0} ms");
        }
    }


    public void IncreaseMoveTime(Position position, TimeSpan searchTimeElapsed, int horizon, ScoredMove[] bestMoveIterations)
    {
        if (!CanIncreaseMoveTime || (horizon < _minToHorizon) || (_timeBankMilliseconds == 0)) return;
        var scoreDecrease = bestMoveIterations[horizon - 1].Score - bestMoveIterations[horizon].Score;
        if (scoreDecrease < _smallScoreDecrease) return;
        
        // Calculate new move times.
        var timePer128 = scoreDecrease >= _largeScoreDecrease ? _largeScoreDecreaseTimePer128 : _smallScoreDecreaseTimePer128;
        var milliseconds = FastMath.Min((_initialSoftLimitMilliseconds * timePer128) / 128, _timeBankMilliseconds);
        MoveTimeSoftLimit += TimeSpan.FromMilliseconds(milliseconds);
        MoveTimeHardLimit += TimeSpan.FromMilliseconds(milliseconds);
        _timeBankMilliseconds -= milliseconds;
        
        if (messenger.Debug)
        {
            messenger.WriteLine($"Increasing move time {milliseconds} milliseconds because score has decreased {scoreDecrease} centipawns from previous search iteration.");
            messenger.WriteLine($"info string MoveTimeSoftLimit = {MoveTimeSoftLimit.TotalMilliseconds:0} ms MoveTimeHardLimit = {MoveTimeHardLimit.TotalMilliseconds:0} ms");
        }

        // Prevent loss on time.
        var timeRemaining = TimeRemaining[(int)position.ColorToMove] ?? throw new Exception($"{nameof(TimeRemaining)} for {position.ColorToMove} is null.");
        timeRemaining -= searchTimeElapsed + _moveTimeReserved;
        var timeIncrement = TimeIncrement[(int)position.ColorToMove] ?? TimeSpan.Zero;
        if (MoveTimeHardLimit > timeRemaining) PreventLossOnTime(timeRemaining, timeIncrement);
    }


    private void PreventLossOnTime(TimeSpan timeRemaining, TimeSpan timeIncrement)
    {
        var movesRemaining = MovesToTimeControl ?? _movesRemainingTimePressure;
        var millisecondsRemaining = timeRemaining.TotalMilliseconds + (movesRemaining * timeIncrement.TotalMilliseconds);
        var moveMilliseconds = FastMath.Min(millisecondsRemaining / movesRemaining, timeRemaining.TotalMilliseconds);

        MoveTimeSoftLimit = TimeSpan.FromMilliseconds(moveMilliseconds);
        MoveTimeHardLimit = MoveTimeSoftLimit;

        if (messenger.Debug)
        {
            messenger.WriteLine("info string Preventing loss on time.");
            messenger.WriteLine($"info string MoveTimeSoftLimit = {MoveTimeSoftLimit.TotalMilliseconds:0} ms MoveTimeHardLimit = {MoveTimeHardLimit.TotalMilliseconds:0} ms");
        }
    }


    public bool HaveTimeForNextHorizon(TimeSpan searchTimeElapsed)
    {
        if (MoveTimeSoftLimit == TimeSpan.MaxValue) return true;

        var moveTimePer128 = ((128 * searchTimeElapsed.TotalMilliseconds) / MoveTimeSoftLimit.TotalMilliseconds);
        return moveTimePer128 <= _haveTimeSearchNextPlyPer128;
    }


    public void Reset()
    {
        // Reset move times and limits.
        TimeRemaining[(int)Color.White] = null;
        TimeRemaining[(int)Color.Black] = null;
        MovesToTimeControl = null;
        HorizonLimit = Search.MaxHorizon;
        NodeLimit = long.MaxValue;
        MateInMoves = null;
        MoveTimeSoftLimit = TimeSpan.MaxValue;
        MoveTimeHardLimit = TimeSpan.MaxValue;
        CanIncreaseMoveTime = true;
    }
}