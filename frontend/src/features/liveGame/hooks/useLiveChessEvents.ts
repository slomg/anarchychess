import { StoreApi, useStore } from "zustand";

import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { decodeMovePath, decodeLegalMoves } from "../lib/moveDecoder";
import AudioPlayer, { AudioType } from "@/features/audio/audioPlayer";
import { Position } from "@/features/chessboard/lib/position";
import { Clocks, GameColor, MoveSnapshot } from "@/lib/apiClient";
import { LiveChessStore } from "../stores/liveChessStore";
import { refetchGame } from "../lib/gameStateProcessor";
import { useGameEvent } from "./useGameHub";

export default function useLiveChessEvents(
    liveChessStore: StoreApi<LiveChessStore>,
    chessboardStore: StoreApi<ChessboardStore>,
) {
    const boardDimensions = useStore(chessboardStore, (x) => x.boardDimensions);
    const gameToken = useStore(liveChessStore, (x) => x.gameToken);

    async function handleMoveUpdate(
        move: MoveSnapshot,
        plyNumber: number,
        sideToMove: GameColor,
        clocks: Clocks,
    ): Promise<Position | undefined> {
        const {
            positionHistory,
            addPosition,
            applyMoveAnimated,
            goToLatestPosition,
        } = chessboardStore.getState();
        const { isPendingMoveAck, receiveLiveMove } = liveChessStore.getState();

        // we missed a move... we need to refetch the state
        if (plyNumber - 1 != positionHistory.mainPlyCount) {
            await refetchGame(liveChessStore, chessboardStore);
            return;
        }
        await goToLatestPosition();

        const decodedMove = decodeMovePath(move.path, boardDimensions.width);
        if (!isPendingMoveAck) {
            await applyMoveAnimated(decodedMove);
        }

        const pieces = chessboardStore.getState().pieces;
        const position = addPosition({
            pieces,
            fen: move.fen,
            movedBy: move.movedBy,
            san: move.san,
            move: decodedMove,
            // clocks: {
            //     whiteClock: clocks.whiteClock,
            //     blackClock: clocks.blackClock,
            // },
        });
        receiveLiveMove(clocks, sideToMove);
        return position;
    }

    useGameEvent(gameToken, "SyncRevisionAsync", async (currentRevision) => {
        const { sourceRevision } = liveChessStore.getState();
        if (sourceRevision !== currentRevision) {
            await refetchGame(liveChessStore, chessboardStore);
        }
    });

    useGameEvent(
        gameToken,
        "MoveMadeAsync",
        async (move, plyNumber, sideToMove, clocks) => {
            const { viewer } = liveChessStore.getState();
            if (viewer.playerColor !== sideToMove) {
                await handleMoveUpdate(move, plyNumber, sideToMove, clocks);
            }
        },
    );

    useGameEvent(
        gameToken,
        "OpponentMoveMadeAsync",
        async (move, plyNumber, encodedLegalMoves, hasForcedMoves, clocks) => {
            const { viewer } = liveChessStore.getState();
            if (viewer.playerColor === null) return;

            const position = await handleMoveUpdate(
                move,
                plyNumber,
                viewer.playerColor,
                clocks,
            );
            if (!position) return;

            const decodedLegalMoves = decodeLegalMoves({
                encoded: encodedLegalMoves,
                boardWidth: boardDimensions.width,
                hasForcedMoves: hasForcedMoves,
            });
            chessboardStore
                .getState()
                .addLegalMoves(decodedLegalMoves, position.positionId);
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
