import { decodeFen } from "@/features/chessboard/lib/fenDecoder";
import PositionHistory from "@/features/chessboard/lib/positionHistory";
import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import { decodeMovePathIntoLegalMoves } from "@/features/liveGame/lib/moveDecoder";
import { GameColor, RootAnalysisPosition } from "@/lib/apiClient";
import constants from "@/lib/constants";
import { StoreApi } from "zustand";

export default function processRootAnalysis(
    position: RootAnalysisPosition,
): StoreApi<ChessboardStore> {
    const boardWidth = constants.BOARD_WIDTH;
    const boardHeight = constants.BOARD_HEIGHT;

    const pieces = decodeFen(position.fen);
    const legalMoves = decodeMovePathIntoLegalMoves({
        paths: position.moveOptions.legalMoves,
        boardWidth,
        hasForcedMoves: position.moveOptions.hasForcedMoves,
    });
    const positionHistory = new PositionHistory(pieces);

    return createChessboardStore({
        pieces,
        positionHistory,
        legalMovesByPosition: new Map([
            [positionHistory.viewingPosition?.positionId, legalMoves],
        ]),

        boardDimensions: {
            width: boardWidth,
            height: boardHeight,
        },
        viewingFrom: GameColor.WHITE,
        allowHistoryChanges: true,
    });
}
