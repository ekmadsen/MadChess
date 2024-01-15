// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2023.       |
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


namespace ErikTheCoder.MadChess.Engine.Intelligence;


public sealed class TimeManagement(Messenger messenger) // Messenger lifetime managed by caller.
{
    public readonly TimeSpan?[] TimeRemaining = new TimeSpan?[2]; // [color]
    public readonly TimeSpan?[] TimeIncrement = new TimeSpan?[2]; // [color]
    public int? MovesToTimeControl;
    public int HorizonLimit;
    public long NodeLimit;
    public int? MateInMoves;
    public TimeSpan MoveTimeSoftLimit;
    public TimeSpan MoveTimeHardLimit;
    public bool CanAdjustMoveTime;

    private const int _movesRemainingDefault = 20;
    private const int _movesRemainingTimePressure = 4;
    private const int _moveTimeHardLimitPer128 = 536;
    private const int _adjustMoveTimeMinHorizon = 9;
    private const int _adjustMoveTimeMinScoreDecrease = 50;
    private const int _adjustMoveTimePer128 = 64;
    private const int _haveTimeSearchNextPlyPer128 = 70;

    private readonly TimeSpan _moveTimeReserved = TimeSpan.FromMilliseconds(100);


    public void DetermineMoveTime(Position position, TimeSpan searchTimeElapsed)
    {
        // No need to calculate move time if go command specifies move time, horizon limit, or nodes.
        if ((MoveTimeHardLimit != TimeSpan.MaxValue) || (HorizonLimit != Search.MaxHorizon) || (NodeLimit != long.MaxValue)) return;

        // Retrieve time remaining, time increment, and moves remaining until next time control.
        var timeRemaining = TimeRemaining[(int)position.ColorToMove] ?? throw new Exception($"{nameof(TimeRemaining)} for {position.ColorToMove} is null.");
        if (timeRemaining == TimeSpan.MaxValue) return; // No need to calculate move time if go command specifies infinite search.
        timeRemaining -= searchTimeElapsed + _moveTimeReserved; // Reserve time to prevent flagging.
        var timeIncrement = TimeIncrement[(int)position.ColorToMove] ?? TimeSpan.Zero;
        var movesRemaining = MovesToTimeControl ?? _movesRemainingDefault;

        // Calculate move time.
        var millisecondsRemaining = timeRemaining.TotalMilliseconds + (movesRemaining * timeIncrement.TotalMilliseconds);
        var milliseconds = millisecondsRemaining / movesRemaining;
        MoveTimeSoftLimit = TimeSpan.FromMilliseconds(milliseconds);
        MoveTimeHardLimit = TimeSpan.FromMilliseconds((milliseconds * _moveTimeHardLimitPer128) / 128);

        if (MoveTimeHardLimit > timeRemaining)
        {
            // Prevent loss on time.
            movesRemaining = MovesToTimeControl ?? _movesRemainingTimePressure;
            millisecondsRemaining = timeRemaining.TotalMilliseconds + (movesRemaining * timeIncrement.TotalMilliseconds);
            milliseconds = FastMath.Min(millisecondsRemaining / movesRemaining, timeRemaining.TotalMilliseconds);
            MoveTimeSoftLimit = TimeSpan.FromMilliseconds(milliseconds);
            MoveTimeHardLimit = timeRemaining;
            if (messenger.Debug) messenger.WriteLine("info string Preventing loss on time.");
        }
        if (messenger.Debug) messenger.WriteLine($"info string Moves Remaining = {movesRemaining} MoveTimeSoftLimit = {MoveTimeSoftLimit.TotalMilliseconds:0} MoveTimeHardLimit = {MoveTimeHardLimit.TotalMilliseconds:0}");
    }


    public void AdjustMoveTime(int originalHorizon, ScoredMove[] bestMovePlies)
    {
        if (!CanAdjustMoveTime || (originalHorizon < _adjustMoveTimeMinHorizon) || (MoveTimeSoftLimit == MoveTimeHardLimit)) return;
        if (bestMovePlies[originalHorizon].Score >= (bestMovePlies[originalHorizon - 1].Score - _adjustMoveTimeMinScoreDecrease)) return;

        // Score has decreased significantly from last ply.
        if (messenger.Debug) messenger.WriteLine("Adjusting move time because score has decreased significantly from previous ply.");
        MoveTimeSoftLimit += TimeSpan.FromMilliseconds((MoveTimeSoftLimit.TotalMilliseconds * _adjustMoveTimePer128) / 128);
        if (MoveTimeSoftLimit > MoveTimeHardLimit) MoveTimeSoftLimit = MoveTimeHardLimit;
    }


    public bool HaveTimeForNextHorizon(TimeSpan searchTimeElapsed)
    {
        if (MoveTimeSoftLimit == TimeSpan.MaxValue) return true;
        var moveTimePer128 = (int)(128 * searchTimeElapsed.TotalMilliseconds / MoveTimeSoftLimit.TotalMilliseconds);
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
        CanAdjustMoveTime = true;
    }
}
