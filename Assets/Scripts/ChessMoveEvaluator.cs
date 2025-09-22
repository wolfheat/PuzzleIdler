using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

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

    private static FullChessMove GetValidFullMove(ChessMove performedMove, ChessPiece[,] chessSetup, int bottomPlayerColor, ChessMove lastPerformedMove, FullChessMove result)
    {
        // Checking valid move does not care if it is the player or oponent that maked the move. Playercolor only sets which color is on which side on the board.

        // Check the distination position in the setup - also check both pieces of same type
        ChessPiece movedPiece = chessSetup[performedMove.from.x, performedMove.from.y];
        ChessPiece targetPiece = chessSetup[performedMove.to.x, performedMove.to.y];

        Debug.Log("Performed Move from = "+performedMove.from.x+","+performedMove.from.y+" type: "+movedPiece.Type);

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

            //Debug.Log("RookMove is valid rowchange = "+rowChange+" colChange = "+colChange);

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
            //Debug.Log("Movedirection: "+moveDirection.x+","+moveDirection.y);

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

            Debug.Log("Checking for valid Castle Move");

            // Also need to check that rook is available and knight and bishop is not
            int rowChange = performedMove.to.y - performedMove.from.y;
            int colChange = performedMove.to.x - performedMove.from.x;

            // Moving up or down the board
            if(rowChange != 0) return false;

            // Not moving like a castle move
            if(Mathf.Abs(colChange) != 2)
                return false;

            
            Debug.Log("Is moveing 2 steps");

            // King and queen spot (depends on rotation of board)
            if(performedMove.from.x != 3 && performedMove.from.x != 4)
                return false;

            Debug.Log("Is moveing from king or queen spot");

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
            int colChange = performedMove.to.x - performedMove.from.x;            
            if (Math.Abs(rowChange) > 1 || Math.Abs(colChange) > 1) 
                return false;
            //Debug.Log("It is a valid normal kings move");
            return true;
        }
        
        bool IsValidPawnMove() // 
        {
            //Debug.Log("Checking valid pawn move");
            int allowedMoveDirection = movedPiece.Color == 0 ? 1 : -1;
            // Inverse if player plays black
            if(bottomPlayerColor == 1) {
                allowedMoveDirection *= -1;
            }

            int rowChange = performedMove.to.y - performedMove.from.y;
            int colChange = performedMove.to.x - performedMove.from.x;

            if(Math.Sign(rowChange) != Math.Sign(allowedMoveDirection)) // Moving in wrong direction
                return false;

            bool twoStepsAllowed = performedMove.from.y == 1 || performedMove.from.y == 6;


            if (Math.Abs(rowChange) != 1) {
                // If step ahead is free
                if(Math.Abs(rowChange) == 2 && twoStepsAllowed && colChange == 0) {
                    if(chessSetup[performedMove.from.x, performedMove.from.y + allowedMoveDirection] != null)
                        return false; // Doesnt step one step forward - invalid
                    // Valid two step initial pawn move
                    result.performed.enPassentable = true;
                    return true;
                }
                return false;
            }

            // Is one step

            if (Math.Abs(colChange) == 1) { // Side Step
                // Take an oponent
                if (chessSetup[performedMove.to.x, performedMove.to.y] == null) {
                    // Check for valid en passent - if available add it as valid
                    //Debug.Log("Check valid en passent,  last move ended at ["+ lastPerformedMove.to.x+","+ lastPerformedMove.to.y+"]");
                    if (lastPerformedMove.enPassentable && (lastPerformedMove.to.x == performedMove.to.x && lastPerformedMove.to.y == performedMove.from.y)){
                        //Debug.Log("is valid en passent");
                        // Valid en passent
                        result.other = new ChessMove(new Vector2Int(lastPerformedMove.to.x, lastPerformedMove.to.y), new Vector2Int(-1, -1));
                        return true;
                    }                    
                    //Debug.Log("is in-valid en passent");
                    return false; // Can only take another piece, except en passent
                }
                // Capture
                return true;
            }
            if (Math.Abs(colChange) == 0) { // Forward Step
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
                    //Debug.Log("IsJumping from "+checkedPos.x+","+checkedPos.y);
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

    internal static bool CheckCheck(ChessMove lastPerformedMove, ChessPiece[,] setup, int playerColor)
    {
        // Enemy king is in check = at least one piece can take the enemy if more than 1 king has to be able to move to a free spot = if only one
        // , no piece can be placed in front of the king to hinder the check
        // King has to be in check

        if(PlayerCanTakeComputerKingFromThisPosition(lastPerformedMove,setup, playerColor))
            return true;
        return false;
    }
    
    internal static bool CheckIfMate(ChessMove lastPerformedMove, ChessPiece[,] setup, int playerColor)
    {
        // Enemy king is in check
        // Generate all possible oponent moves and check for still mate in all
        // Generate all positions after all possible moves
                
        // Players indexes for piexes are
        for (int j = 0; j < 8; j++) {
            for (int i = 0; i < 8; i++) {   
                if (setup[j, i] == null) continue;
                if (setup[j, i].Type == -1) continue;
                if (playerColor == 1 ? setup[j, i].Color < 6 : setup[j, i].Type >= 6) {

                    // this is a oponent piece

                    // Try all possible moves
                    //Debug.Log("TRYING ALL POSSIBLE MOVES");

                    // Evaluate if still check in all new positions

                    bool inCheckInAllSetups = DoesAllValidMovesKeepOpponentInCheck(lastPerformedMove,setup, playerColor);

                    //if (PlayerCanTakeComputerKingFromThisPosition(lastPerformedMove,setup, playerColor))
                    //    return true;

                    return inCheckInAllSetups;
                }
            }
        }
        return false;


    }

    private static bool DoesAllValidMovesKeepOpponentInCheck(ChessMove lastPerformedMove ,ChessPiece[,] setup, int playerColor)
    {
        //Debug.Log("Player is "+playerColor);

        // Generate all available moves for each piece and add that position and check for still checked

        for (int j = 0; j < 8; j++) {
            for (int i = 0; i < 8; i++) {
                if (setup[j, i] == null) continue;
                if (setup[j, i].Type == -1) continue;

                if (playerColor == 0 ? IsBlackPiece(setup[j, i].Type) : IsWhitePiece(setup[j, i].Type)) {
                    // this is the opponents piece - try move it to all valid positions

                    Vector2Int pos = new Vector2Int(j, i);

                    List<ChessMove> playersMoves = GetAllValidChessMoves(pos, lastPerformedMove, setup, playerColor);

                    
                    foreach (ChessMove move in playersMoves) {
                        // Check if the Move is valid - Also get any changes as a FullChessMove        
                        FullChessMove fullChessMove = ChessMoveEvaluator.Evaluate(move, lastPerformedMove, setup, playerColor);// Check if it is a valid move - Validation needs current setup and the move to be made. If validating en passent also last move is needed (if we want to return players piece as if it is an illegal move when trying to do en passent)

                        if (!fullChessMove.valid) continue;

                        // Generate the new setup
                        ChessPiece[,] newSetup = GetNewSetupAfterMove(setup, fullChessMove);

                        if (!PlayerCanTakeComputerKingFromThisPosition(move, newSetup, playerColor))
                            return false;

                    }

                    // Check the resulting position if check


                }
            }
        }
        return true;
    }

    private static ChessPiece[,] GetNewSetupAfterMove(ChessPiece[,] setup, FullChessMove fullChessMove)
    {
        // Make this move and return the new setup

        ChessPiece[,] newSetup = (ChessPiece[,])setup.Clone();

        ChessPiece movedPiece = newSetup[fullChessMove.performed.from.x, fullChessMove.performed.from.y];

        newSetup[fullChessMove.performed.from.x, fullChessMove.performed.from.y] = null;

        newSetup[fullChessMove.performed.to.x, fullChessMove.performed.to.y] = movedPiece;


        // Also remove any en passented piece

        return newSetup;

    }

    private static List<ChessMove> GetAllValidChessMoves(Vector2Int piecePos, ChessMove lastPerformedMove, ChessPiece[,] setup, int playerColor)
    {
        // From this position get all available moves
        ChessPiece movedPiece = setup[piecePos.x,piecePos.y];
        int color = movedPiece.Color;

        Vector2Int checkPosition = new();

        List<ChessMove> moves = movedPiece.Type switch
        {
            0 or 6 => GetListOfValidMovesByRook(),
            1 or 7 => GetListOfValidMovesByKnight(),
            2 or 8 => GetListOfValidMovesByBishop(),
            3 or 9 => GetListOfValidMovesByQueen(),
            4 or 10 => GetListOfValidMovesByKing(),
            _ => GetListOfValidMovesByPawn()
        };
        
        return moves;

        // LOCAL METHODS

        bool IsValidEndPos(Vector2Int posChecked)
        {
            if(posChecked.x < 0 || posChecked.x > 7 || posChecked.y < 0 || posChecked.y > 7) return false;
            if (setup[posChecked.x,posChecked.y] == null) return true;
            return (setup[posChecked.x, posChecked.y].Color != color);
        }

        // Local Validat Methods - Can use the supplied move color and setup
        List<ChessMove> GetListOfValidMovesByKnight()
        {       
            List<ChessMove> moves = new List<ChessMove>();
            List<Vector2Int> endPositions = new List<Vector2Int>() { new Vector2Int(-2, 1), new Vector2Int(-1, 2), new Vector2Int(1, 2), new Vector2Int(2, 1), new Vector2Int(-2, -1), new Vector2Int(-1, -2), new Vector2Int(1, -2), new Vector2Int(2, -1) };

            for (int i = 0; i < endPositions.Count; i++) {
                checkPosition = piecePos + endPositions[i];
                if (IsValidEndPos(checkPosition))
                    moves.Add(new ChessMove(piecePos, checkPosition));
            }
            return moves;
        }
        
        // Local Validat Methods - Can use the supplied move color and setup
        List<ChessMove> GetListOfValidMovesByBishop()
        {       
            List<ChessMove> moves = new List<ChessMove>();


            List<Vector2Int> dir = new List<Vector2Int>() { new Vector2Int(-1, -1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(1, 1) };

            for (int i = 0; i < dir.Count; i++) {
                checkPosition = piecePos + dir[i];
                while (IsValidEndPos(checkPosition)) {
                    ChessMove move = new ChessMove(piecePos, checkPosition);
                    // Invalid - Dropped on a piece of same color
                    if (IsPieceDroppedOnTopOfOwnPiece(move, setup))
                        break;

                    moves.Add(move);
                    //If on top of enemy this is last position but still valid
                    if (setup[checkPosition.x, checkPosition.y] != null && setup[checkPosition.x, checkPosition.y].Type != -1)
                        break;
                    checkPosition += dir[i];
                }
            }
            return moves;
        }
        
        // Local Validat Methods - Can use the supplied move color and setup
        List<ChessMove> GetListOfValidMovesByRook()
        {       
            List<ChessMove> moves = new List<ChessMove>();

            List<Vector2Int> dir = new List<Vector2Int>() { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };

            for (int i = 0; i<dir.Count; i++) {
                checkPosition = piecePos + dir[i];
                while (IsValidEndPos(checkPosition)) {
                    ChessMove move = new ChessMove(piecePos, checkPosition);
                    // Invalid - Dropped on a piece of same color
                    if (IsPieceDroppedOnTopOfOwnPiece(move, setup))
                        break;

                    moves.Add(move);
                    //If on top of enemy this is last position but still valid
                    if (setup[checkPosition.x, checkPosition.y] != null && setup[checkPosition.x, checkPosition.y].Type != -1)
                        break;
                    checkPosition += dir[i];
                }
            }
            return moves;
        }
        
        // Local Validat Methods - Can use the supplied move color and setup
        List<ChessMove> GetListOfValidMovesByQueen()
        {
            List<ChessMove> moves = GetListOfValidMovesByRook();
            moves.AddRange(GetListOfValidMovesByBishop());
            return moves;
        }
        
        // Local Validat Methods - Can use the supplied move color and setup
        List<ChessMove> GetListOfValidMovesByPawn()
        {
            List<ChessMove> moves = new List<ChessMove>();
            List<Vector2Int> endPositions = new List<Vector2Int>();
            // Depends on player or enemy making the move?
            if(playerColor != color) {
                // Moving down on the board

                // List with one step in it
                endPositions.Add(new Vector2Int(0, -1));

                // on startPosition = can take 2 step
                if (piecePos.y == 6 && setup[piecePos.x,piecePos.y-1] == null && setup[piecePos.x, piecePos.y - 2] == null) // Can only go two steps if free spot on both positions
                    endPositions.Add(new Vector2Int(0,-2));

                // Can take oponent to left or right?
                if (piecePos.x > 0 && setup[piecePos.x - 1, piecePos.y - 1] != null && setup[piecePos.x - 1, piecePos.y - 1].Color != color)
                    endPositions.Add(new Vector2Int(-1,-1));
                if (piecePos.x < 7 && setup[piecePos.x + 1, piecePos.y - 1] != null && setup[piecePos.x + 1, piecePos.y - 1].Color != color)
                    endPositions.Add(new Vector2Int(1,-1));

                // Also check for en passent capture


            }

            for (int i = 0; i < endPositions.Count; i++) {
                checkPosition = piecePos + endPositions[i];
                if (IsValidEndPos(checkPosition))
                    moves.Add(new ChessMove(piecePos, checkPosition));
            }
            return moves;
        }

        
        // Local Validat Methods - Can use the supplied move color and setup
        List<ChessMove> GetListOfValidMovesByKing()
        {
            List<ChessMove> moves = new List<ChessMove>();
            List<Vector2Int> endPositions = new List<Vector2Int>() { new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1) };

            for (int i = 0; i < endPositions.Count; i++) {
                checkPosition = piecePos + endPositions[i];
                if (IsValidEndPos(checkPosition))
                    moves.Add(new ChessMove(piecePos, checkPosition));
            }
            return moves;
        }




    }

    private static bool PlayerCanTakeComputerKingFromThisPosition(ChessMove lastPerformedMove, ChessPiece[,] setup, int playerColor)
    {
        // Get all players pieces
        int[,] positions = new int[8,8];

        Vector2Int oponentKingPos = new Vector2Int();

        ChessMove additionalMove = null;
        FullChessMove result;

        for (int j = 0; j < 8; j++) {
            for (int i = 0; i < 8; i++) {
                positions[j,i] = setup[j,i]?.Type ?? -1;
                if (playerColor == 0 ? positions[j, i] == 10 : positions[j, i] == 4)
                    oponentKingPos = new Vector2Int(j, i);
            }
        }

        // Players indexes for piexes are
        for (int j = 0; j < 8; j++) {
            for (int i = 0; i < 8; i++) {
                if (positions[j,i]==-1) continue;
                if(playerColor == 0 ? positions[j, i] < 6 : positions[j, i] >= 6) {
                    // this is a player piece

                    Vector2Int fromPos = new Vector2Int(j,i);

                    // Make a move to take the king form here
                    ChessMove move = new ChessMove(fromPos, oponentKingPos, 0);

                    result = new FullChessMove(move, additionalMove, false);
                                        
                    // Is piece going to an unreachable square
                    if (GetValidFullMove(move, setup, playerColor, lastPerformedMove, result).valid) {
                        return true;
                    }
                }
            }
        }
        return false;        
    }

    private static List<Vector2Int> GetTargetPoints(int pieceID, int[,] positions, Vector2Int piecePos, int playerColor)
    {
        // Figure out what points this picece can target
        List<Vector2Int> targets = new List<Vector2Int>();

        targets = pieceID switch
        {
            0 or 6 => GetRookTargets(),
            1 or 7 => GetKnightTargets(),
            //2 or 8 => IsValidBishopMove(),
            //3 or 9 => IsValidQueenMove(),
            //4 or 10 => IsValidKingMove() || IsValidCastleMove(),
            _ => GetPawnTargets()
        };

        List<Vector2Int> GetKnightTargets()
        {
            // Move in all directions to get all empty spots and any enemy piece and stop there
            Vector2Int checkPosition = piecePos;

            List<Vector2Int> ans = new();

            
            checkPosition = piecePos + new Vector2Int(-2,1); // Left Up
            AddIfTargetedOponentAtPosition(positions, checkPosition, playerColor, ref ans);
            checkPosition = piecePos + new Vector2Int(-1,2); // Up Left
            AddIfTargetedOponentAtPosition(positions, checkPosition, playerColor, ref ans);
            checkPosition = piecePos + new Vector2Int( 1,2); // Up Right
            AddIfTargetedOponentAtPosition(positions, checkPosition, playerColor, ref ans);
            checkPosition = piecePos + new Vector2Int( 2,1); // Right Up
            AddIfTargetedOponentAtPosition(positions, checkPosition, playerColor, ref ans);

            checkPosition = piecePos + new Vector2Int(-2,-1); // Left Down
            AddIfTargetedOponentAtPosition(positions, checkPosition, playerColor, ref ans);
            checkPosition = piecePos + new Vector2Int(-1,-2); // Down Left
            AddIfTargetedOponentAtPosition(positions, checkPosition, playerColor, ref ans);
            checkPosition = piecePos + new Vector2Int( 1,-2); // Down Right
            AddIfTargetedOponentAtPosition(positions, checkPosition, playerColor, ref ans);
            checkPosition = piecePos + new Vector2Int( 2,-1); // Right Down
            AddIfTargetedOponentAtPosition(positions, checkPosition, playerColor, ref ans);

            return ans;

        }
        
        List<Vector2Int> GetRookTargets()
        {
            // Move in all directions to get all empty spots and any enemy piece and stop there
            Vector2Int checkPosition = piecePos;

            List<Vector2Int> ans = new();

            // Move right
            checkPosition = piecePos;
            while (checkPosition.x < 7) {
                checkPosition = new Vector2Int(checkPosition.x - 1, checkPosition.y);
                if(AddIfTargetedOponentAtPosition(positions, checkPosition, playerColor, ref ans))
                    break;
            }
            
            // Move left
            checkPosition = piecePos;
            while (checkPosition.x > 0) {
                checkPosition = new Vector2Int(checkPosition.x - 1, checkPosition.y);
                if (AddIfTargetedOponentAtPosition(positions, checkPosition, playerColor, ref ans))
                    break;
            }

            // Move up
            checkPosition = piecePos;
            while (checkPosition.y < 7) {
                checkPosition = new Vector2Int(checkPosition.x, checkPosition.y + 1);
                if (AddIfTargetedOponentAtPosition(positions, checkPosition, playerColor, ref ans))
                    break;
            }

            // Down
            checkPosition = piecePos;
            while (checkPosition.y > 0) {
                checkPosition = new Vector2Int(checkPosition.x, checkPosition.y - 1);
                if (AddIfTargetedOponentAtPosition(positions, checkPosition, playerColor, ref ans))
                    break;
            }

            return ans;

        }
        
        List<Vector2Int> GetPawnTargets()
        {
            // Move in all directions to get all empty spots and any enemy piece and stop there
            Vector2Int checkPosition = piecePos;

            List<Vector2Int> ans = new();

            // Move Ahead-Left
            if(piecePos.x > 0) {
                AddIfTargetedOponentAtPosition(positions, new Vector2Int(checkPosition.x - 1, checkPosition.y), playerColor, ref ans);
            }
            // Move Ahead-Right
            checkPosition = piecePos;
            
            if(piecePos.x < 7) {
                AddIfTargetedOponentAtPosition(positions, new Vector2Int(checkPosition.x + 1, checkPosition.y), playerColor, ref ans);
            }
            
            return ans;
        }



        return targets;
    }

    private static bool AddIfTargetedOponentAtPosition(int[,] positions, Vector2Int checkPosition, int playerColor, ref List<Vector2Int> ans)
    {
        int pieceAtPos = positions[checkPosition.x, checkPosition.y];
        if (pieceAtPos == -1) {
            // Add this
            ans.Add(checkPosition);
        }
        else if (playerColor == 0 ? IsBlackPiece(pieceAtPos) : IsWhitePiece(pieceAtPos)) {
            // this is the opponents piece so can be captured
            ans.Add(checkPosition);
            return true; // stop searching if hitting a piece
        }else
            return true; // stop searching if hitting a piece
        return false;
    }


    private static bool IsWhitePiece(int pieceAtPos)
    {
        return pieceAtPos < 6;
    }

    private static bool IsBlackPiece(int pieceAtPos)
    {
        return pieceAtPos >= 6;
    }
}
