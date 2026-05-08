"use client";

import { StoreApi } from "zustand";
import { useMemo } from "react";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";

import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import ChessboardLayout, { ChessboardLayoutProps } from "./ChessboardLayout";
import createDefaultChessboard from "../lib/defaultBoard";
import useBoardReplay from "../hooks/useBoardReplay";
import PositionHistory from "../lib/positionHistory";
import { decodeFen } from "../lib/fenDecoder";
import BoardPieces from "../lib/boardPieces";
import { GameColor } from "@/lib/apiClient";
import { GameReplay } from "../lib/types";
import useConst from "@/hooks/useConst";

interface BaseChessboardProps {
    viewingFrom?: GameColor;
    disableDrag?: boolean;
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
    viewingFrom = GameColor.WHITE,
    disableDrag = false,
    muteAudio = false,

    position = createDefaultChessboard(),
    replays = [],

    ...props
}: ChessboardProps & ChessboardLayoutProps) => {
    const initialPosition = useMemo(
        () =>
            replays.length
                ? decodeFen(replays[0].startingFen).pieces
                : position,
        [replays, position],
    );

    const chessboardStore = useConst<StoreApi<ChessboardStore>>(() =>
        createChessboardStore({
            pieces: initialPosition,
            legalMovesByPosition: new Map(),
            positionHistory: new PositionHistory({ pieces: initialPosition }),
            viewingFrom,
            disableDrag,
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
