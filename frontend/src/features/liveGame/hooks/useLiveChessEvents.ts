import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { Clocks, GameColor, MoveSnapshot } from "@/lib/apiClient";
import { StoreApi, useStore } from "zustand";
import { LiveChessStore } from "../stores/liveChessStore";
import { decodeMovePath, decodeLegalMoves } from "../lib/moveDecoder";
import { refetchGame } from "../lib/gameStateProcessor";
import { useGameEvent } from "./useGameHub";
import AudioPlayer, { AudioType } from "@/features/audio/audioPlayer";
import { PositionProps } from "@/features/chessboard/lib/positionHistory";

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

    useGameEvent(
        gameToken,
        "MoveMadeAsync",
        async (move, sideToMove, plyNumber, clocks) => {
            const { isPendingMoveAck, viewer, receiveMove } =
                liveChessStore.getState();
            const {
                positionHistory,
                addLatestPosition,
                applyMoveAnimated,
                disableMovement,
            } = chessboardStore.getState();

            // we missed a move... we need to refetch the state
            if (plyNumber - 1 != positionHistory.mainPlyCount) {
                await refetchGame(liveChessStore, chessboardStore);
                return;
            }

            if (viewer.playerColor !== sideToMove) {
                disableMovement();
            }

            const decodedMove = decodeMovePath(
                move.path,
                boardDimensions.width,
            );
            if (!isPendingMoveAck) {
                await applyMoveAnimated(decodedMove);
            }

            const pieces = chessboardStore.getState().pieces;
            const position: PositionProps = {
                pieces,
                san: move.san,
                move: decodedMove,
                // clocks: {
                //     whiteClock: clocks.whiteClock,
                //     blackClock: clocks.blackClock,
                // },
            };

            addLatestPosition(position);
            receiveMove(clocks, sideToMove);
        },
    );

    useGameEvent(
        gameToken,
        "LegalMovesChangedAsync",
        async (encodedLegalMoves, hasForcedMoves, plyNumber) => {
            const decodedLegalMoves = decodeLegalMoves({
                encoded: encodedLegalMoves,
                boardWidth: boardDimensions.width,
                hasForcedMoves: hasForcedMoves,
            });

            // chessboardStore
            //     .getState()
            //     .addLegalMoves(decodedLegalMoves, plyNumber + 1); // plyNumber + 1 because our history includes the starting position
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
