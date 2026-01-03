import { StoreApi } from "zustand";
import { useEffect } from "react";

import {
    GameColor,
    getNextAnalysisPosition,
    RootAnalysisPosition,
} from "@/lib/apiClient";

import { decodeMovePathIntoLegalMoves } from "@/features/liveGame/lib/moveDecoder";
import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { Move } from "@/features/chessboard/lib/types";

export default function useAnalysisMoveEmitter(
    rootPosition: RootAnalysisPosition,
    chessboardStore: StoreApi<ChessboardStore>,
) {
    const { pieceMovementEvent, addPosition, addLegalMoves } =
        chessboardStore.getState();

    useEffect(() => {
        async function emitMove(move: Move) {
            const { pieces, boardDimensions, positionHistory, goToPosition } =
                chessboardStore.getState();

            const nextPosition = positionHistory.getNextPositionWithKey(
                move.moveKey,
            );
            if (nextPosition) {
                await goToPosition(nextPosition.positionId);
                return;
            }

            const { error, data } = await getNextAnalysisPosition({
                body: {
                    fen:
                        positionHistory.viewingPosition?.fen ??
                        rootPosition.fen,
                    movingPlayer:
                        positionHistory.viewingPosition?.sideToMove ??
                        GameColor.WHITE,
                    piecePosition: move.from,
                    moveKey: move.moveKey,
                },
            });
            if (error || data === undefined) {
                console.error(error);
                return;
            }

            const position = addPosition({
                pieces,
                move,
                sideToMove: data.sideToMove,
                fen: data.fen,
                san: data.san,
            });

            const decodedLegalMoves = decodeMovePathIntoLegalMoves({
                paths: data.moveOptions.legalMoves,
                boardWidth: boardDimensions.width,
                hasForcedMoves: data.moveOptions.hasForcedMoves,
            });
            addLegalMoves(decodedLegalMoves, position.positionId);
        }

        pieceMovementEvent.subscribe(emitMove);
        return () => {
            pieceMovementEvent.unsubscribe(emitMove);
        };
    }, [
        pieceMovementEvent,
        addPosition,
        addLegalMoves,
        chessboardStore,
        rootPosition.fen,
    ]);
}
