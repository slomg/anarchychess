"use client";

import { StoreApi } from "zustand";
import { useMemo } from "react";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";

import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import ChessboardLayout, { ChessboardLayoutProps } from "./ChessboardLayout";
import useBoardReplay from "../hooks/useBoardReplay";
import { decodeFen } from "../lib/fenDecoder";
import BoardPieces from "../lib/boardPieces";
import { GameColor } from "@/lib/apiClient";
import { GameReplay } from "../lib/types";
import constants from "@/lib/constants";
import useConst from "@/hooks/useConst";

interface BaseChessboardProps {
    boardWidth?: number;
    boardHeight?: number;
    viewingFrom?: GameColor;
    canDrag?: boolean;
    muteAudio?: boolean;
}

interface ChessboardPropsWithReplay extends BaseChessboardProps {
    replays: GameReplay[];
    position?: never;
}

interface ChessboardPropsWithPosition extends BaseChessboardProps {
    replays?: never;
    position?: BoardPieces;
}

type ChessboardProps = ChessboardPropsWithReplay | ChessboardPropsWithPosition;

const StaticChessboard = ({
    boardHeight = constants.BOARD_HEIGHT,
    boardWidth = constants.BOARD_WIDTH,
    viewingFrom = GameColor.WHITE,
    canDrag = true,
    muteAudio = false,

    position = constants.DEFAULT_CHESS_BOARD,
    replays = [],

    ...props
}: ChessboardProps & ChessboardLayoutProps) => {
    const initialPosition = useMemo(
        () => (replays.length ? decodeFen(replays[0].startingFen) : position),
        [replays, position],
    );

    const chessboardStore = useConst<StoreApi<ChessboardStore>>(() =>
        createChessboardStore({
            pieces: initialPosition,
            boardDimensions: { width: boardWidth, height: boardHeight },
            legalMovesByPly: new Map(),
            viewingFrom,
            canDrag,
            muteAudio,
        }),
    );
    useBoardReplay(replays, chessboardStore);

    return (
        <ChessboardStoreContext.Provider value={chessboardStore}>
            <ChessboardLayout {...props} />
        </ChessboardStoreContext.Provider>
    );
};

export default StaticChessboard;
