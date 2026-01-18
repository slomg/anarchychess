import { StoreApi, useStore } from "zustand";

import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { decodeMovePath, decodeLegalMoves } from "../lib/moveDecoder";
import AudioPlayer, { AudioType } from "@/features/audio/audioPlayer";
import { Position } from "@/features/chessboard/lib/position";
import { Clocks, MoveSnapshot } from "@/lib/apiClient";
import { LiveChessStore } from "../stores/liveChessStore";
import { refetchGame } from "../lib/gameStateProcessor";
import { useGameEvent } from "./useGameHub";
import LegalMoves from "@/features/chessboard/lib/legalMoves";

export default function useLiveChessEvents(
    liveChessStore: StoreApi<LiveChessStore>,
    chessboardStore: StoreApi<ChessboardStore>,
) {
    const boardDimensions = useStore(chessboardStore, (x) => x.boardDimensions);
    const gameToken = useStore(liveChessStore, (x) => x.gameToken);

    async function handleMoveUpdate(
        move: MoveSnapshot,
        plyNumber: number,
        clocks: Clocks,
        legalMoves?: LegalMoves,
    ): Promise<Position | undefined> {
        const {
            positionHistory,
            addPosition,
            applyMoveAnimated,
            goToLatestPosition,
            reselectPiece,
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
        const position = addPosition(
            {
                pieces,
                fen: move.fen,
                sideToMove: move.nextSideToMove,
                san: move.san,
                move: decodedMove,
                // clocks: {
                //     whiteClock: clocks.whiteClock,
                //     blackClock: clocks.blackClock,
                // },
            },
            legalMoves,
        );
        reselectPiece();
        receiveLiveMove(clocks, move.nextSideToMove);
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
        async (move, plyNumber, clocks) => {
            const { viewer } = liveChessStore.getState();
            if (viewer.playerColor !== move.nextSideToMove) {
                await handleMoveUpdate(move, plyNumber, clocks);
            }
        },
    );

    useGameEvent(
        gameToken,
        "OpponentMoveMadeAsync",
        async (move, plyNumber, encodedLegalMoves, clocks) => {
            const { viewer } = liveChessStore.getState();
            if (viewer.playerColor === null) return;

            const decodedLegalMoves = decodeLegalMoves({
                encoded: encodedLegalMoves,
                boardWidth: boardDimensions.width,
            });
            await handleMoveUpdate(move, plyNumber, clocks, decodedLegalMoves);
        },
    );

    useGameEvent(gameToken, "DrawStateChangeAsync", (drawState) =>
        liveChessStore.getState().drawStateChange(drawState),
    );

    useGameEvent(gameToken, "GameEndedAsync", async (result, finalClocks) => {
        liveChessStore.getState().endGame(result, finalClocks);
        chessboardStore.getState().setAllowHistoryChanges(true);
        AudioPlayer.playAudio(AudioType.GAME_END);
    });
}
