import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";

import { decodeMovePathIntoLegalMoves } from "@/features/liveGame/lib/moveDecoder";
import PositionHistory from "@/features/chessboard/lib/positionHistory";
import { GameColor, RootAnalysisPosition } from "@/lib/apiClient";
import { decodeFen } from "@/features/chessboard/lib/fenDecoder";

export default function processRootAnalysis(
    position: RootAnalysisPosition,
): StoreApi<ChessboardStore> {
    const pieces = decodeFen(position.fen);
    const legalMoves = decodeMovePathIntoLegalMoves(position.legalMoves);
    const positionHistory = new PositionHistory(pieces, position.fen);

    return createChessboardStore({
        pieces,
        positionHistory,
        legalMovesByPosition: new Map([
            [positionHistory.viewingPosition?.positionId, legalMoves],
        ]),

        viewingFrom: GameColor.WHITE,
        allowHistoryChanges: true,
    });
}
