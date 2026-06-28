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
    private const int _incrementTimePer128 = 64;
    private const int _moveTimeHardLimitPer128 = 512;
    private const int _movesRemainingTimePressure = 4;
    private const int _increaseMoveTimeMinToHorizon = 15;
    private const int _increaseMoveTimeMinScoreDecrease = 33;
    private const int _increaseMoveTimePer128 = 128;
    private const int _haveTimeSearchNextPlyPer128 = 70;

    private readonly TimeSpan _moveTimeReserved = TimeSpan.FromMilliseconds(100);


    public void DetermineMoveTime(Position position, TimeSpan searchTimeElapsed)
    {
        // No need to calculate move time if go command specifies move time, horizon limit, or nodes.
        if ((MoveTimeHardLimit != TimeSpan.MaxValue) || (HorizonLimit != Search.MaxHorizon) || (NodeLimit != long.MaxValue)) return;

        // Retrieve time remaining and time increment.
        var timeRemaining = TimeRemaining[(int)position.ColorToMove] ?? throw new Exception($"{nameof(TimeRemaining)} for {position.ColorToMove} is null.");
        if (timeRemaining == TimeSpan.MaxValue) return; // No need to calculate move time if go command specifies infinite search.
        timeRemaining -= searchTimeElapsed + _moveTimeReserved; // Reserve time to prevent flagging (losing on time).
        var timeIncrement = TimeIncrement[(int)position.ColorToMove] ?? TimeSpan.Zero;

        // Calculate move time.
        double moveMilliseconds;
        double millisecondsRemaining;
        if (MovesToTimeControl.HasValue)
        {
            // Specific number of moves must be made before time expires.
            millisecondsRemaining = timeRemaining.TotalMilliseconds + (MovesToTimeControl.Value * timeIncrement.TotalMilliseconds);
            moveMilliseconds = millisecondsRemaining / MovesToTimeControl.Value;
        }
        else
        {
            // Game must be completed before time expires.
            moveMilliseconds = (timeRemaining.TotalMilliseconds * _moveTimePer1024) / 1024;
            moveMilliseconds += (timeIncrement.TotalMilliseconds * _incrementTimePer128) / 128;
        }

        MoveTimeSoftLimit = TimeSpan.FromMilliseconds(moveMilliseconds);
        MoveTimeHardLimit = TimeSpan.FromMilliseconds((moveMilliseconds * _moveTimeHardLimitPer128) / 128);

        if (MoveTimeHardLimit > timeRemaining)
        {
            // Prevent flagging.
            var movesRemaining = MovesToTimeControl ?? _movesRemainingTimePressure;
            millisecondsRemaining = timeRemaining.TotalMilliseconds + (movesRemaining * timeIncrement.TotalMilliseconds);
            moveMilliseconds = FastMath.Min(millisecondsRemaining / movesRemaining, timeRemaining.TotalMilliseconds);

            MoveTimeSoftLimit = TimeSpan.FromMilliseconds(moveMilliseconds);
            MoveTimeHardLimit = MoveTimeSoftLimit;
            if (messenger.Debug) messenger.WriteLine("info string Preventing loss on time.");
        }

        if (messenger.Debug)
        {
            messenger.WriteLine($"info string TimeRemaining = {timeRemaining.TotalMilliseconds:0} ms TimeIncrement = {timeIncrement.TotalMilliseconds:0} ms MovesToTimeControl = {MovesToTimeControl}");
            messenger.WriteLine($"info string MoveTimeSoftLimit = {MoveTimeSoftLimit.TotalMilliseconds:0} ms MoveTimeHardLimit = {MoveTimeHardLimit.TotalMilliseconds:0} ms");
        }
    }


    public void IncreaseMoveTime(int originalHorizon, ScoredMove[] bestMovePlies)
    {
        if (!CanIncreaseMoveTime || (originalHorizon < _increaseMoveTimeMinToHorizon) || (MoveTimeSoftLimit == MoveTimeHardLimit)) return;
        if (bestMovePlies[originalHorizon].Score >= (bestMovePlies[originalHorizon - 1].Score - _increaseMoveTimeMinScoreDecrease)) return;

        // TODO: Increase move time more significantly if score decreases >= 200 centipawns from previous search (search, not iteration) and crosses from positive to negative.

        // Score has decreased marginally from previous iteration.
        if (messenger.Debug) messenger.WriteLine("Increasing move time because score has decreased significantly from previous ply.");
        MoveTimeSoftLimit += TimeSpan.FromMilliseconds((MoveTimeSoftLimit.TotalMilliseconds * _increaseMoveTimePer128) / 128);
        if (MoveTimeSoftLimit > MoveTimeHardLimit) MoveTimeSoftLimit = MoveTimeHardLimit;
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
    }
}