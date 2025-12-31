import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { StoreApi, useStore } from "zustand";
import { LiveChessStore } from "../stores/liveChessStore";
import { decodeMovePath, decodeLegalMoves } from "../lib/moveDecoder";
import { refetchGame } from "../lib/gameStateProcessor";
import { useGameEvent } from "./useGameHub";
import AudioPlayer, { AudioType } from "@/features/audio/audioPlayer";
import { PositionId } from "@/features/chessboard/lib/positionHistory";
import { useRef } from "react";
import LegalMoves from "@/features/chessboard/lib/legalMoves";

export default function useLiveChessEvents(
    liveChessStore: StoreApi<LiveChessStore>,
    chessboardStore: StoreApi<ChessboardStore>,
) {
    const boardDimensions = useStore(chessboardStore, (x) => x.boardDimensions);
    const gameToken = useStore(liveChessStore, (x) => x.gameToken);

    useGameEvent(gameToken, "SyncRevisionAsync", async (currentRevision) => {
        const { sourceRevision } = liveChessStore.getState();
        if (sourceRevision !== currentRevision) {
            await refetchGame(liveChessStore, chessboardStore);
        }
    });

    const pendingLegalMovesRef = useRef<LegalMoves | null>(null);
    const liveHeadPositionId = useRef<PositionId | null>(null);

    useGameEvent(
        gameToken,
        "MoveMadeAsync",
        async (move, sideToMove, plyNumber, clocks) => {
            const { isPendingMoveAck, viewer, receiveMove } =
                liveChessStore.getState();
            const {
                positionHistory,
                addPosition,
                applyMoveAnimated,
                disableMovement,
                setLatestLegalMoves,
                goToLatestPosition,
            } = chessboardStore.getState();

            // we missed a move... we need to refetch the state
            if (plyNumber - 1 != positionHistory.mainPlyCount) {
                await refetchGame(liveChessStore, chessboardStore);
                return;
            }

            const isOurTurn = viewer.playerColor === sideToMove;
            if (!isOurTurn) {
                disableMovement();
            }
            await goToLatestPosition();

            const decodedMove = decodeMovePath(
                move.path,
                boardDimensions.width,
            );
            if (!isPendingMoveAck) {
                await applyMoveAnimated(decodedMove);
            }

            const pieces = chessboardStore.getState().pieces;
            const position = addPosition({
                pieces,
                san: move.san,
                move: decodedMove,
                // clocks: {
                //     whiteClock: clocks.whiteClock,
                //     blackClock: clocks.blackClock,
                // },
            });
            receiveMove(clocks, sideToMove);

            if (pendingLegalMovesRef.current) {
                setLatestLegalMoves(pendingLegalMovesRef.current);
                pendingLegalMovesRef.current = null;
            } else if (isOurTurn) {
                liveHeadPositionId.current = position.positionId;
            }
        },
    );

    useGameEvent(
        gameToken,
        "LegalMovesChangedAsync",
        async (encodedLegalMoves, hasForcedMoves) => {
            const decodedLegalMoves = decodeLegalMoves({
                encoded: encodedLegalMoves,
                boardWidth: boardDimensions.width,
                hasForcedMoves: hasForcedMoves,
            });

            if (liveHeadPositionId.current) {
                chessboardStore
                    .getState()
                    .addLegalMoves(
                        decodedLegalMoves,
                        liveHeadPositionId.current,
                    );
                liveHeadPositionId.current = null;
            } else {
                pendingLegalMovesRef.current = decodedLegalMoves;
            }
        },
    );

    useGameEvent(gameToken, "DrawStateChangeAsync", (drawState) =>
        liveChessStore.getState().drawStateChange(drawState),
    );

    useGameEvent(gameToken, "GameEndedAsync", async (result, finalClocks) => {
        liveChessStore.getState().endGame(result, finalClocks);
        chessboardStore.getState().disableMovement();
        AudioPlayer.playAudio(AudioType.GAME_END);
    });
}
