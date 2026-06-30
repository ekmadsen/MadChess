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

    private const int _moveTimePer1024 = 50;
    private const int _incrementTimePer128 = 96;
    private const int _moveTimeInitialHardLimitPer128 = 384;
    private const int _moveTimeHardLimitPer128 = 768;
    private const int _movesRemainingTimePressure = 4;
    private const int _increaseMoveTimeMinToHorizon = 13;
    private const int _increaseMoveTimeMarginalScoreDecrease = 30;
    private const int _increaseMoveTimeMarginalPer128 = 128;
    private const int _increaseMoveTimeSignificantScoreDecrease = 150;
    private const int _increaseMoveTimeSignificantPer128 = 256;
    private const int _haveTimeSearchNextPlyPer128 = 70;

    private readonly TimeSpan _moveTimeReserved = TimeSpan.FromMilliseconds(100);
    private double _softLimitMilliseconds;
    private double _hardLimitMilliseconds;


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
        double moveMilliseconds;
        if (MovesToTimeControl.HasValue)
        {
            // Specific number of moves must be made before time expires.
            var millisecondsRemaining = timeRemaining.TotalMilliseconds + (MovesToTimeControl.Value * timeIncrement.TotalMilliseconds);
            moveMilliseconds = millisecondsRemaining / MovesToTimeControl.Value;
        }
        else
        {
            // Game must be completed before time expires.
            moveMilliseconds = (timeRemaining.TotalMilliseconds * _moveTimePer1024) / 1024;
            moveMilliseconds += (timeIncrement.TotalMilliseconds * _incrementTimePer128) / 128;
        }

        MoveTimeSoftLimit = TimeSpan.FromMilliseconds(moveMilliseconds);
        MoveTimeHardLimit = TimeSpan.FromMilliseconds((moveMilliseconds * _moveTimeInitialHardLimitPer128) / 128);
        _softLimitMilliseconds = moveMilliseconds;
        _hardLimitMilliseconds = (moveMilliseconds * _moveTimeHardLimitPer128) / 128;

        if (MoveTimeHardLimit > timeRemaining) PreventLossOnTime(timeRemaining, timeIncrement);

        if (messenger.Debug)
        {
            messenger.WriteLine($"info string TimeRemaining = {timeRemaining.TotalMilliseconds:0} ms TimeIncrement = {timeIncrement.TotalMilliseconds:0} ms MovesToTimeControl = {MovesToTimeControl}");
            messenger.WriteLine($"info string MoveTimeSoftLimit = {MoveTimeSoftLimit.TotalMilliseconds:0} ms MoveTimeHardLimit = {MoveTimeHardLimit.TotalMilliseconds:0} ms");
        }
    }


    public void IncreaseMoveTime(Position position, TimeSpan searchTimeElapsed, int originalHorizon, ScoredMove[] bestMoveIterations)
    {
        if (!CanIncreaseMoveTime || (originalHorizon < _increaseMoveTimeMinToHorizon) || (MoveTimeSoftLimit.TotalMilliseconds >= _hardLimitMilliseconds)) return;
        var scoreDecrease = bestMoveIterations[originalHorizon - 1].Score - bestMoveIterations[originalHorizon].Score;
        if (scoreDecrease < _increaseMoveTimeMarginalScoreDecrease) return;

        // Calculate new move times.
        double millisecondsIncrease;
        if (scoreDecrease >= _increaseMoveTimeSignificantScoreDecrease)
        {
            // Score has decreased significantly from previous iteration.
            if (messenger.Debug) messenger.WriteLine("Increasing move time because score has decreased significantly from previous search iteration.");
            millisecondsIncrease = (_softLimitMilliseconds * _increaseMoveTimeSignificantPer128) / 128;
        }
        else
        {
            // Score has decreased marginally from previous iteration.
            if (messenger.Debug) messenger.WriteLine("Increasing move time because score has decreased marginally from previous search iteration.");
            millisecondsIncrease = (_softLimitMilliseconds * _increaseMoveTimeMarginalPer128) / 128;
        }
        MoveTimeSoftLimit += TimeSpan.FromMilliseconds(millisecondsIncrease);
        MoveTimeHardLimit += TimeSpan.FromMilliseconds(millisecondsIncrease);

        // Verify move times are within limits.
        if (MoveTimeHardLimit.TotalMilliseconds > _hardLimitMilliseconds) MoveTimeHardLimit = TimeSpan.FromMilliseconds(_hardLimitMilliseconds);
        if (messenger.Debug) messenger.WriteLine($"info string MoveTimeSoftLimit = {MoveTimeSoftLimit.TotalMilliseconds:0} ms MoveTimeHardLimit = {MoveTimeHardLimit.TotalMilliseconds:0} ms");

        var timeRemaining = TimeRemaining[(int)position.ColorToMove] ?? throw new Exception($"{nameof(TimeRemaining)} for {position.ColorToMove} is null.");
        timeRemaining -= searchTimeElapsed + _moveTimeReserved;
        var timeIncrement = TimeIncrement[(int)position.ColorToMove] ?? TimeSpan.Zero;
        if (MoveTimeHardLimit > timeRemaining) PreventLossOnTime(timeRemaining, timeIncrement);

        if (MoveTimeSoftLimit > MoveTimeHardLimit) MoveTimeSoftLimit = MoveTimeHardLimit;
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
        var moveTimePer128 = (int)((128 * searchTimeElapsed.TotalMilliseconds) / MoveTimeSoftLimit.TotalMilliseconds);
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
        _softLimitMilliseconds = double.MaxValue;
        _hardLimitMilliseconds = double.MaxValue;
    }
}