// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2019.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace ErikTheCoder.MadChess.Engine
{
    public sealed class PgnGame
    {
        public readonly int Number;
        public readonly GameResult Result;
        private readonly string _notation;
        private readonly char[] _buffer;
        private string _cleanNotation;
        private List<string> _standardAlgebraicMoves;
        private List<string> _longAlgebraicMoves;
        public List<ulong> Moves;


        public PgnGame(Board Board, int Number, GameResult Result, string Notation)
        {
            this.Number = Number;
            this.Result = Result;
            _notation = Notation;
            _buffer = new char[1];
            ParseStandardAlgebraicMoves();
            UpdateMoves(Board);
        }


        private void ParseStandardAlgebraicMoves()
        {
            if (_notation == null) return;
            // Remove tags, comments, and variations from notation.
            StringBuilder stringBuilder = new StringBuilder();
            using (StringReader stringReader = new StringReader(_notation))
            {
                // Remove tags.
                do
                {
                    string line = stringReader.ReadLine();
                    if (line == null) break;
                    if (line.StartsWith("[") && line.EndsWith("]")) continue; // Skip tag.
                    break; // End of tag section.
                } while (true);
                // Remove comments and variations.
                do
                {
                    int charsRead = stringReader.Read(_buffer, 0, 1);
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
                            // Don't include carriage return in clean notation.
                            stringBuilder.Append(' ');
                            break;
                        case '\n':
                            // Don't include newline in clean notation.
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
            string[] moves = _cleanNotation.Split(". ".ToCharArray());
            _standardAlgebraicMoves = new List<string>(moves.Length);
            for (int moveIndex = 0; moveIndex < moves.Length; moveIndex++)
            {
                string move = moves[moveIndex];
                string cleanMove = move?.Trim();
                if (string.IsNullOrEmpty(cleanMove)) continue;
                char firstCharacter = cleanMove[0];
                if (char.IsNumber(firstCharacter)) continue; // Skip move number or result.
                if (firstCharacter == '*') continue; // Skip unknown result.
                // Add move to list.
                _standardAlgebraicMoves.Add(cleanMove);
            }
        }


        private void UpdateMoves(Board Board)
        {
            _longAlgebraicMoves = new List<string>(_standardAlgebraicMoves.Count);
            Moves = new List<ulong>(_standardAlgebraicMoves.Count);
            Board.SetPosition(Board.StartPositionFen);
            for (int moveIndex = 0; moveIndex < _standardAlgebraicMoves.Count; moveIndex++)
            {
                string standardAlgebraicMove = _standardAlgebraicMoves[moveIndex];
                ulong move;
                try 
                {
                    move = Move.ParseStandardAlgebraic(Board, standardAlgebraicMove);
                }
                catch (Exception exception)
                {
                    throw new Exception($"Error updating {standardAlgebraicMove} move in game {Number}.{Environment.NewLine}{Board.CurrentPosition}", exception);
                }
                string longAlgebraicMove = Move.ToLongAlgebraic(move);
                // Determine if move is legal.
                if (!Board.IsMoveLegal(ref move)) throw new Exception($"Move {longAlgebraicMove} is illegal in position {Board.CurrentPosition.ToFen()}."); // Move is illegal.
                _longAlgebraicMoves.Add(longAlgebraicMove);
                Moves.Add(move);
                Board.PlayMove(move);
            }
            _cleanNotation = null;
            _standardAlgebraicMoves = null;
            _longAlgebraicMoves = null;
        }


        private void ReadToSectionEnd(TextReader StringReader, char OpeningChar, char ClosingChar)
        {
            int sections = 1;
            do
            {
                int charsRead = StringReader.Read(_buffer, 0, 1);
                if (charsRead == 0) break;
                if (_buffer[0] == OpeningChar) sections++;
                else if (_buffer[0] == ClosingChar)
                {
                    sections--;
                    if (sections == 0) break;
                }
            } while (true);
        }


        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
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
}
