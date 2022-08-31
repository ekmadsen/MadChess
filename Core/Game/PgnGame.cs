// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2022.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using ErikTheCoder.MadChess.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ErikTheCoder.MadChess.Core.Moves;


namespace ErikTheCoder.MadChess.Core.Game;


public sealed class PgnGame
{
    // ReSharper disable once MemberCanBePrivate.Global
    public readonly int Number;
    public readonly GameResult Result;
    public List<ulong> Moves;
    private readonly string _notation;
    private readonly char[] _buffer;
    private string _cleanNotation;
    private List<string> _standardAlgebraicMoves;
    private List<string> _longAlgebraicMoves;
        

    public PgnGame(Board board, int number, GameResult result, string notation)
    {
        Number = number;
        Result = result;
        _notation = notation;
        _buffer = new char[1];
        ParseStandardAlgebraicMoves();
        UpdateMoves(board);
    }


    private void ParseStandardAlgebraicMoves()
    {
        if (_notation == null) return;
        // Remove tags, comments, and variations from notation.
        var stringBuilder = new StringBuilder();
        using (var stringReader = new StringReader(_notation))
        {
            // Remove tags.
            do
            {
                var line = stringReader.ReadLine();
                if (line == null) break;
                if (line.StartsWith("[") && line.EndsWith("]")) continue; // Skip tag.
                break; // End of tag section.
            } while (true);
            // Remove comments and variations.
            do
            {
                var charsRead = stringReader.Read(_buffer, 0, 1);
                if (charsRead == 0) break;
                switch (_buffer[0])
                {
                    case '{':
                        // Found a comment.
                        ReadToSectionEnd(stringReader, '{', '}');
                        break;
                    case '(':
                        // Found a variation.
                        ReadToSectionEnd(stringReader, '(', ')');
                        break;
                    case '\r':
                        // Do not include carriage return in clean notation.
                        stringBuilder.Append(' ');
                        break;
                    case '\n':
                        // Do not include newline in clean notation.
                        stringBuilder.Append(' ');
                        break;
                    default:
                        stringBuilder.Append(_buffer[0]);
                        break;
                }
            } while (true);
            _cleanNotation = stringBuilder.ToString().Trim();
        }
        // Read moves from notation.
        var moves = _cleanNotation.Split(". ".ToCharArray());
        _standardAlgebraicMoves = new List<string>(moves.Length);
        for (var moveIndex = 0; moveIndex < moves.Length; moveIndex++)
        {
            var move = moves[moveIndex];
            var cleanMove = move.Trim();
            if (cleanMove.IsNullOrEmpty()) continue;
            var firstCharacter = cleanMove[0];
            if (char.IsNumber(firstCharacter)) continue; // Skip move number or result.
            if (firstCharacter == '*') continue; // Skip unknown result.
            // Add move to list.
            _standardAlgebraicMoves.Add(cleanMove);
        }
    }


    private void UpdateMoves(Board board)
    {
        _longAlgebraicMoves = new List<string>(_standardAlgebraicMoves.Count);
        Moves = new List<ulong>(_standardAlgebraicMoves.Count);
        board.SetPosition(Board.StartPositionFen);
        for (var moveIndex = 0; moveIndex < _standardAlgebraicMoves.Count; moveIndex++)
        {
            var standardAlgebraicMove = _standardAlgebraicMoves[moveIndex];
            ulong move;
            try 
            {
                move = Move.ParseStandardAlgebraic(board, standardAlgebraicMove);
            }
            catch (Exception exception)
            {
                throw new Exception($"Error updating {standardAlgebraicMove} move in game {Number}.{Environment.NewLine}{board.CurrentPosition}", exception);
            }
            var longAlgebraicMove = Move.ToLongAlgebraic(move);
            _longAlgebraicMoves.Add(longAlgebraicMove);
            Moves.Add(move);
            var (legalMove, _) = board.PlayMove(move);
            if (!legalMove) throw new Exception($"Move {longAlgebraicMove} is illegal in position {board.PreviousPosition.ToFen()}.");
        }
        _cleanNotation = null;
        _standardAlgebraicMoves = null;
        _longAlgebraicMoves = null;
    }


    private void ReadToSectionEnd(TextReader stringReader, char openingChar, char closingChar)
    {
        var sections = 1;
        do
        {
            var charsRead = stringReader.Read(_buffer, 0, 1);
            if (charsRead == 0) break;
            if (_buffer[0] == openingChar) sections++;
            else if (_buffer[0] == closingChar)
            {
                sections--;
                if (sections == 0) break;
            }
        } while (true);
    }


    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("Number");
        stringBuilder.AppendLine("======");
        stringBuilder.AppendLine(Number.ToString());
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("Result");
        stringBuilder.AppendLine("======");
        stringBuilder.AppendLine(Result.ToString());
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("Notation");
        stringBuilder.AppendLine("========");
        stringBuilder.AppendLine(_notation);
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("Clean Notation");
        stringBuilder.AppendLine("==============");
        stringBuilder.AppendLine(_cleanNotation);
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("Standard Algebraic Moves");
        stringBuilder.AppendLine("========================");
        stringBuilder.AppendLine(string.Join(" ", _standardAlgebraicMoves));
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("Long Algebraic Moves");
        stringBuilder.AppendLine("====================");
        stringBuilder.AppendLine(string.Join(" ", _longAlgebraicMoves));
        return stringBuilder.ToString();
    }
}