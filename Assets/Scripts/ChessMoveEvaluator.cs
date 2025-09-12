using System;
using UnityEngine;

public static class ChessMoveEvaluator
{
    // Evaluate takes in the chessmove to perform, the previous chessMove nullable and the current chess setup
    // Returns a bool isValid, FullChessMove containing performed chessmove and any other piece affected (also nullable)
    public static FullChessMove Evaluate(ChessMove performedMove, ChessMove lastPerformedMove, ChessPiece[,] chessSetup, int playerColor = 0)
    {
        ChessMove additionalMove = null;
        FullChessMove result = new FullChessMove(performedMove,additionalMove,false);

        // Is this move valid on the current chess Setup with the last Performed move included (only for EP) - disregards previous castle check limitation moves

        // Invalid - Dropped on a piece of same color
        if(IsPieceDroppedOnTopOfOwnPiece(performedMove,chessSetup))
            return result;

        // Is piece going to an unreachable square
        result = GetValidFullMove(performedMove, chessSetup, playerColor, lastPerformedMove, result);
        
        return result;         

    }

    private static FullChessMove GetValidFullMove(ChessMove performedMove, ChessPiece[,] chessSetup, int playerColor, ChessMove lastPerformedMove, FullChessMove result)
    {
        // Check the distination position in the setup - also check both pieces of same type
        ChessPiece movedPiece = chessSetup[performedMove.from.x, performedMove.from.y];
        ChessPiece targetPiece = chessSetup[performedMove.to.x, performedMove.to.y];

        Debug.Log("Performed Move from = "+performedMove.from.x+","+performedMove.from.y);

        // Sets the other piece for deletion by setting it to -1, -1 
        result.other = targetPiece != null ? new ChessMove(new Vector2Int(performedMove.to.x, performedMove.to.y), new Vector2Int(-1, -1)) : null;
        // Need to set this specific for en passent

        int pieceColor = movedPiece.Color;
        int squareStartColor = (1 + performedMove.from.x + performedMove.from.y) % 2;
        int squareEndColor = (1 + performedMove.to.x + performedMove.to.y) % 2;

        result.valid = movedPiece.Type switch {
            0 or 6 => IsValidRookMove(),
            1 or 7 => IsValidKnightMove(),
            2 or 8 => IsValidBishopMove(),
            3 or 9 => IsValidQueenMove(),
            4 or 10=> IsValidKingMove() || IsValidCastleMove(),
            _ => IsValidPawnMove()
        };
        // If any landing on another piece it is removed
        if (chessSetup[performedMove.to.x,performedMove.to.y] != null) {
            // Normal capture
            result.other = new ChessMove(new Vector2Int(performedMove.to.x, performedMove.to.y), new Vector2Int(-1, -1));
        }

        return result;

        // Local Validat Methods - Can use the supplied move color and setup
        bool IsValidRookMove()
        {
            int rowChange = performedMove.to.y - performedMove.from.y;
            int colChange = performedMove.to.x - performedMove.from.x;
            Vector2Int moveDirection = new Vector2Int(colChange == 0 ? 0 : (colChange < 0 ? -1 : 1), rowChange == 0 ? 0 : (rowChange < 0 ? -1 : 1));
            int amtSteps = Math.Max(Math.Abs(rowChange), Math.Abs(colChange));

            // Any Rook move is valid that only changes row or column and doesnt jump over any piece, and caputers only opopnents piece
            if(rowChange != 0 && colChange != 0) // If both row and column change - invalid move
                return false; 
            if(IsJumping(moveDirection, amtSteps)) // If piece needs to jump to get there - Invalid move
                return false;

            Debug.Log("RookMove is valid rowchange = "+rowChange+" colChange = "+colChange);

            return true;
        }

        // Local Validat Methods - Can use the supplied move color and setup
        bool IsValidKnightMove()
        {
            int rowChange = performedMove.to.y - performedMove.from.y;
            int colChange = performedMove.to.x - performedMove.from.x;
            return ((Math.Abs(rowChange) == 1 && Math.Abs(colChange) == 2)||(Math.Abs(rowChange) == 2 && Math.Abs(colChange) == 1));
        }
        // Local Validat Methods - Can use the supplied move color and setup
        bool IsValidBishopMove()
        {
            int rowChange = performedMove.to.y - performedMove.from.y;
            int colChange = performedMove.to.x - performedMove.from.x;
            Vector2Int moveDirection = new Vector2Int(Math.Sign(colChange), Math.Sign(rowChange));
            Debug.Log("Movedirection: "+moveDirection.x+","+moveDirection.y);

            int amtSteps = Math.Abs(rowChange);
            if (Math.Abs(rowChange) != Math.Abs(colChange)) // If both change in row and column are not the same - invalid move
                return false;

            if (IsJumping(moveDirection, amtSteps)) // If piece needs to jump to get there - Invalid move
                return false;
            return true;
        }
        // Local Validat Methods - Can use the supplied move color and setup
        bool IsValidQueenMove() => IsValidBishopMove() || IsValidRookMove();

        bool IsValidCastleMove()
        {
            // Depends on rotation of board ie playerColor

            // To begin with just check for two steps from kings or queens square
            
            // Also need to check that rook is available and knight and bishop is not
            int rowChange = performedMove.to.y - performedMove.from.y;
            int colChange = performedMove.to.x - performedMove.from.x;

            // Moving up or down the board
            if(rowChange != 0) return false;

            // Not moving like a castle move
            if(colChange != 2)
                return false;

            // King and queen spot (depends on rotation of board)
            if(performedMove.from.x != 3 || performedMove.from.x != 4)
                return false;

            // Check all versions of castle
            // Can't have knight or bishop but need rook
            if (performedMove.from.x == 3 && performedMove.to.x == 1) { // player black short castle
                if (chessSetup[2, performedMove.from.y] != null || chessSetup[0, performedMove.from.y] == null)
                    return false;
                // Add the rook move as other move
                result.other = new ChessMove(new Vector2Int(0, performedMove.from.y), new Vector2Int(2, performedMove.from.y));
                return true;
            }
            else if (performedMove.from.x == 3 && performedMove.to.x == 5) { // player black long castle
                if (chessSetup[4, performedMove.from.y] != null || chessSetup[6, performedMove.from.y] != null || chessSetup[7, performedMove.from.y] == null)
                    return false;
                // Add the rook move as other move
                result.other = new ChessMove(new Vector2Int(7, performedMove.from.y), new Vector2Int(4, performedMove.from.y));
                return true;
            }
            else if (performedMove.from.x == 4 && performedMove.to.x == 2) { // player white long castle
                if (chessSetup[1, performedMove.from.y] != null || chessSetup[3, performedMove.from.y] != null || chessSetup[0, performedMove.from.y] == null)
                    return false;
                // Add the rook move as other move
                result.other = new ChessMove(new Vector2Int(0, performedMove.from.y), new Vector2Int(3, performedMove.from.y));
                return true;
            }
            else if (performedMove.from.x == 4 && performedMove.to.x == 6) { // player white short castle
                if (chessSetup[5, performedMove.from.y] != null || chessSetup[7, performedMove.from.y] == null)
                    return false;
                // Add the rook move as other move
                result.other = new ChessMove(new Vector2Int(7, performedMove.from.y), new Vector2Int(5, performedMove.from.y));
                return true;
            }
            return false;
        }
        
        bool IsValidKingMove() // OBS - Does not check for stepping into check
        {
            int rowChange = performedMove.to.y - performedMove.from.y;
            int colChange = performedMove.to.y - performedMove.from.y;            
            if (Math.Abs(rowChange) > 1 || Math.Abs(colChange) > 1) 
                return false;
            return true;
        }
        
        bool IsValidPawnMove() // 
        {
            int allowedMoveDirection = movedPiece.Color == 0 ? 1 : -1;
            // Inverse if player plays black
            if(playerColor == 1) {
                allowedMoveDirection *= -1;
            }

            int rowChange = performedMove.to.y - performedMove.from.y;
            int colChange = performedMove.to.x - performedMove.from.x;

            if(Math.Sign(rowChange) != Math.Sign(allowedMoveDirection)) // Moving in wrong direction
                return false;

            bool twoStepsAllowed = performedMove.from.y == 1 || performedMove.from.y == 6;


            if (Math.Abs(rowChange) != 1) {
                // If step ahead is free
                if(Math.Abs(rowChange) == 2 && twoStepsAllowed) {
                    if(chessSetup[performedMove.from.x, performedMove.from.y + allowedMoveDirection] != null)
                        return false; // Doesnt step one step forward - invalid
                    // Valid two step initial pawn move
                    result.performed.enPassentable = true;
                    return true;
                }
                return false;
            }

            // Is one step

            if (Math.Abs(colChange) == 1) {
                // Take an oponent
                if (chessSetup[performedMove.to.x, performedMove.to.y] == null) {
                    // Check for valid en passent - if available add it as valid
                    if (lastPerformedMove.enPassentable && lastPerformedMove.to.x == performedMove.to.x){
                        // Valid en passent
                        result.other = new ChessMove(new Vector2Int(lastPerformedMove.to.x, lastPerformedMove.to.y), new Vector2Int(-1, -1));
                        return true;
                    }
                    return false; // Can only take another piece, except en passent
                }
                return true;
            }
            if (Math.Abs(colChange) == 0) {
                if(chessSetup[performedMove.from.x, performedMove.from.y + allowedMoveDirection] != null) // Can't capture ahead
                    return false;
                return true; 
            }
            return false;
        }

        // Helper Methods
        // Jumps if ther is any piece where piece moves
        bool IsJumping(Vector2Int moveDirection, int steps)
            {
                Vector2Int checkedPos = performedMove.from;
                for (int i = 1; i < steps; i++) {
                    checkedPos = performedMove.from + moveDirection * i;
                    Debug.Log("IsJumping from "+checkedPos.x+","+checkedPos.y);
                    if (chessSetup[checkedPos.x,checkedPos.y] != null)
                        return true;
                }
                return false;
            }


    }

    private static bool IsPieceDroppedOnTopOfOwnPiece(ChessMove performedMove, ChessPiece[,] chessSetup)
    {
        // Check the distination position in the setup - also check both pieces of same type
        ChessPiece movedPiece = chessSetup[performedMove.from.x, performedMove.from.y];
        ChessPiece droppedOnPiece = chessSetup[performedMove.to.x, performedMove.to.y];
        return droppedOnPiece == null ? false : (movedPiece.Color == droppedOnPiece.Color);
    }

    public static bool Equal(ChessMove moveA,ChessMove moveB) => 
        moveA.from.x == moveB.from.x && 
        moveA.from.y == moveB.from.y && 
        moveA.to.x == moveB.to.x && 
        moveA.to.y == moveB.to.y && 
        moveA.promote == moveB.promote;
}
