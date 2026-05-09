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
    createUiLegalMovesSlice,
    UiLegalMovesSlice,
    UiLegalMovesSliceProps,
} from "./uiLegalMovesSlice";
import {
    createBoardEffectsSlice,
    BoardEffectsSlice,
} from "./boardEffectsSlice";
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
import { createSetupModeSlice, SetupModeSlice } from "./setupModeSlice";
import { OverlaySlice, createOverlaySlice } from "./overlaySlice";
import { createPromptSlice, PromptSlice } from "./promptSlice";
import { createThrowSlice, ThrowSlice } from "./throwSlice";
import { CoreSlice, createCoreSlice } from "./coreSlice";
import PositionHistory from "../lib/positionHistory";
import BoardPieces from "../lib/boardPieces";
import { GameColor } from "@/lib/apiClient";

export type ChessboardStore = BoardSlice &
    PiecesSlice &
    PromotionSlice &
    UiLegalMovesSlice &
    HistorySlice &
    OverlaySlice &
    InteractionSlice &
    IntermediateSlice &
    PromptSlice &
    AnimationSlice &
    AudioSlice &
    CoreSlice &
    ThrowSlice &
    BoardEffectsSlice &
    SetupModeSlice;
export type ChessboardProps = BoardSliceProps &
    PieceSliceProps &
    HistorySliceProps &
    AudioSliceProps &
    AnimationSliceProps &
    UiLegalMovesSliceProps;

enableMapSet();
export function createChessboardStore(
    initState: ChessboardProps = {
        viewingFrom: GameColor.WHITE,
        pieces: new BoardPieces(),
        positionHistory: new PositionHistory({ pieces: new BoardPieces() }),
        legalMovesByPosition: new Map(),
    },
) {
    return createWithEqualityFn<ChessboardStore>()(
        devtools(
            immer((...a) => ({
                ...createBoardSlice(initState)(...a),
                ...createPiecesSlice(initState)(...a),
                ...createPromotionSlice(...a),
                ...createUiLegalMovesSlice(initState)(...a),
                ...createHistorySlice(initState)(...a),
                ...createOverlaySlice(...a),
                ...createInteractionSlice(...a),
                ...createIntermediateSlice(...a),
                ...createPromptSlice(...a),
                ...createAnimationSlice(initState)(...a),
                ...createAudioSlice(initState)(...a),
                ...createCoreSlice(...a),
                ...createThrowSlice(...a),
                ...createBoardEffectsSlice(...a),
                ...createSetupModeSlice(...a),
            })),
            { name: "chessboardStore" },
        ),
        shallow,
    );
}
