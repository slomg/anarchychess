import { StoreApi, useStore } from "zustand";
import { useRef } from "react";

import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { decodeMovePath, decodeLegalMoves } from "../lib/moveDecoder";
import AudioPlayer, { AudioType } from "@/features/audio/audioPlayer";
import { Position } from "@/features/chessboard/lib/position";
import LegalMoves from "@/features/chessboard/lib/legalMoves";
import { LiveChessStore } from "../stores/liveChessStore";
import { refetchGame } from "../lib/gameStateProcessor";
import { Clocks, MoveSnapshot } from "@/lib/apiClient";
import { LogicalPoint } from "@/features/point/types";
import { useGameEvent } from "./useGameHub";

export default function useLiveChessEvents(
    liveChessStore: StoreApi<LiveChessStore>,
    chessboardStore: StoreApi<ChessboardStore>,
) {
    const boardDimensions = useStore(chessboardStore, (x) => x.boardDimensions);
    const gameToken = useStore(liveChessStore, (x) => x.gameToken);

    const queuedOvertimeRef = useRef<
        Map<
            number,
            { overtimeRemovals: LogicalPoint[]; legalMoves: LegalMoves }
        >
    >(new Map());

    async function handleMoveUpdate({
        move,
        plyNumber,
        clocks,
        legalMoves,
    }: {
        move: MoveSnapshot;
        plyNumber: number;
        clocks: Clocks;
        legalMoves?: LegalMoves;
    }): Promise<Position | undefined> {
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

        const queuedOvertime = queuedOvertimeRef.current.get(plyNumber);
        if (queuedOvertime) {
            legalMoves = queuedOvertime.legalMoves;
            decodedMove.overtimeRemovals.push(
                ...queuedOvertime.overtimeRemovals,
            );
            queuedOvertimeRef.current.delete(plyNumber);
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
        async (move, plyNumber, clocks, didMoveEndGame) => {
            const { viewer } = liveChessStore.getState();
            if (viewer.playerColor === move.nextSideToMove) return;

            // undefined = we don't know the legal moves
            // defined, but empty = we know the legal moves, there aren't any, no need to fetch them
            const legalMoves = didMoveEndGame
                ? LegalMoves.StableEmpty
                : undefined;
            await handleMoveUpdate({ move, plyNumber, clocks, legalMoves });
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
            await handleMoveUpdate({
                move,
                plyNumber,
                clocks,
                legalMoves: decodedLegalMoves,
            });
        },
    );

    useGameEvent(gameToken, "DrawStateChangeAsync", (drawState) =>
        liveChessStore.getState().drawStateChange(drawState),
    );

    useGameEvent(
        gameToken,
        "ReceiveOvertimeAsync",
        async (plyNumber, removedFrom, encodedLegalMoves) => {
            const { removePieceAt, addLegalMovesForPosition, positionHistory } =
                chessboardStore.getState();

            const legalMoves = decodeLegalMoves({
                encoded: encodedLegalMoves,
                boardWidth: boardDimensions.width,
            });

            const plyDiff = plyNumber - positionHistory.mainPlyCount;
            if (plyDiff > 1) {
                console.warn(
                    `Received overtime for ply ${plyNumber}, which is ahead of current main ply ${positionHistory.mainPlyCount} by more than 1`,
                );
                await refetchGame(liveChessStore, chessboardStore);
                return;
            } else if (plyDiff === 1) {
                const queue = queuedOvertimeRef.current.get(plyNumber) ?? {
                    overtimeRemovals: [],
                    legalMoves,
                };

                queue.legalMoves = legalMoves;
                queue.overtimeRemovals.push(removedFrom);
                queuedOvertimeRef.current.set(plyNumber, queue);
                return;
            }

            const position = positionHistory.getPositionWithPly(plyNumber);
            if (!position) {
                return;
            }

            addLegalMovesForPosition(legalMoves, position.positionId);
            position.commitOvertimeRemoval(removedFrom);

            if (
                position.positionId ===
                positionHistory.viewingPosition?.positionId
            ) {
                removePieceAt(removedFrom);
            }
        },
    );

    useGameEvent(gameToken, "ReceiveErrorAsync", async () => {
        await refetchGame(liveChessStore, chessboardStore);
    });

    useGameEvent(gameToken, "GameEndedAsync", async (result, finalClocks) => {
        liveChessStore.getState().endGame(result, finalClocks);
        chessboardStore.getState().setAllowHistoryChanges(true);
        AudioPlayer.playAudio(AudioType.GAME_END);
    });
}
