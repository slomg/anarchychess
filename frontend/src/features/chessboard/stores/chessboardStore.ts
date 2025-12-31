import { createWithEqualityFn } from "zustand/traditional";
import { immer } from "zustand/middleware/immer";
import { devtools } from "zustand/middleware";
import { shallow } from "zustand/shallow";
import { enableMapSet } from "immer";

import {
    createIntermediateSlice,
    IntermediateSlice,
} from "./intermediateSlice";
import {
    createLegalMovesSlice,
    LegalMovesSlice,
    LegalMovesSliceProps,
} from "./legalMovesSlice";
import {
    AnimationSlice,
    AnimationSliceProps,
    createAnimationSlice,
} from "./animationSlice";
import {
    createHistorySlice,
    HistorySlice,
    HistorySliceProps,
} from "./historySlice";

import { createPiecesSlice, PieceSliceProps, PiecesSlice } from "./piecesSlice";
import { createInteractionSlice, InteractionSlice } from "./interactionSlice";
import { BoardSlice, BoardSliceProps, createBoardSlice } from "./boardSlice";
import { AudioSlice, AudioSliceProps, createAudioSlice } from "./audioSlice";
import { createPromotionSlice, PromotionSlice } from "./promotionSlice";
import { OverlaySlice, createOverlaySlice } from "./overlaySlice";
import { CoreSlice, createCoreSlice } from "./coreSlice";
import BoardPieces from "../lib/boardPieces";
import { GameColor } from "@/lib/apiClient";
import constants from "@/lib/constants";
import PositionHistory from "../lib/positionHistory";

export type ChessboardStore = BoardSlice &
    PiecesSlice &
    PromotionSlice &
    LegalMovesSlice &
    HistorySlice &
    OverlaySlice &
    InteractionSlice &
    IntermediateSlice &
    AnimationSlice &
    AudioSlice &
    CoreSlice;
export type ChessboardProps = BoardSliceProps &
    PieceSliceProps &
    HistorySliceProps &
    AudioSliceProps &
    AnimationSliceProps &
    LegalMovesSliceProps;

enableMapSet();
export function createChessboardStore(
    initState: ChessboardProps = {
        viewingFrom: GameColor.WHITE,
        boardDimensions: {
            width: constants.BOARD_WIDTH,
            height: constants.BOARD_HEIGHT,
        },
        pieces: new BoardPieces(),
        positionHistory: new PositionHistory(new BoardPieces()),
        canDrag: true,
        legalMovesByPosition: new Map(),
        muteAudio: false,
    },
) {
    return createWithEqualityFn<ChessboardStore>()(
        devtools(
            immer((...a) => ({
                ...createBoardSlice(initState)(...a),
                ...createPiecesSlice(initState)(...a),
                ...createPromotionSlice(...a),
                ...createLegalMovesSlice(initState)(...a),
                ...createHistorySlice(initState)(...a),
                ...createOverlaySlice(...a),
                ...createInteractionSlice(...a),
                ...createIntermediateSlice(...a),
                ...createAnimationSlice(initState)(...a),
                ...createAudioSlice(initState)(...a),
                ...createCoreSlice(...a),
            })),
            { name: "chessboardStore" },
        ),
        shallow,
    );
}
