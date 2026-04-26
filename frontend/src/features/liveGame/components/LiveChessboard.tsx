"use client";

import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import {
    processGameState,
    ProcessedGameState,
} from "../lib/gameStateProcessor";

import useEnsureLegalMovesForViewedPosition from "../hooks/useEnsureLegalMovesForViewedPosition";
import MoveHistoryTable from "@/features/chessboard/components/moveHistory/MoveHistoryTable";
import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import ChessboardWithSidebar from "@/features/chessboard/components/ChessboardWithSidebar";
import ChessboardLayout from "@/features/chessboard/components/ChessboardLayout";
import createLiveChessStore, { LiveChessStore } from "../stores/liveChessStore";
import useSyncBoardInteraction from "../hooks/useSyncBoardInteraction";
import LiveChessboardProfile, { ProfileSide } from "./LiveChessboardProfile";
import { useSessionUser } from "@/features/auth/hooks/useSessionUser";
import useInvalidateOnNavigate from "@/hooks/useInvalidateOnNavigate";
import LiveChessStoreContext from "../contexts/liveChessContext";
import GameControlsCard from "./GameControls/GameControlsCard";
import useLiveMoveEmitter from "../hooks/useLiveMoveEmitter";
import useLiveChessEvents from "../hooks/useLiveChessEvents";
import { GameState, Preferences } from "@/lib/apiClient";
import LiveGameOverPopup from "./LiveGameOverPopup";
import OvertimeAlert from "./OvertimeAlert";
import useConst from "@/hooks/useConst";
import GameChat from "./GameChat";

const LiveChessboard = ({
    gameToken,
    gameState,
    preferences,
}: {
    gameToken: string;
    gameState: GameState;
    preferences: Preferences;
}) => {
    const user = useSessionUser();

    const storeProps = useConst<ProcessedGameState>(() =>
        processGameState(gameToken, user?.userId ?? "", gameState),
    );

    const liveChessStore = useConst<StoreApi<LiveChessStore>>(() =>
        createLiveChessStore(storeProps.live),
    );

    const chessboardStore = useConst<StoreApi<ChessboardStore>>(() =>
        createChessboardStore(storeProps.board),
    );

    useEnsureLegalMovesForViewedPosition(gameState.initialFen, chessboardStore);
    useSyncBoardInteraction(liveChessStore, chessboardStore);
    useLiveChessEvents(liveChessStore, chessboardStore);
    useLiveMoveEmitter(liveChessStore, chessboardStore);
    useInvalidateOnNavigate();

    return (
        <LiveChessStoreContext.Provider value={liveChessStore}>
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <LiveGameOverPopup />
                <ChessboardWithSidebar
                    chessboard={
                        <>
                            <LiveChessboardProfile
                                side={ProfileSide.Opponent}
                            />
                            <ChessboardLayout
                                breakpoints={[
                                    {
                                        maxScreenSize: 767,
                                        paddingOffset: {
                                            width: 40,
                                            height: 258,
                                        },
                                    },
                                    {
                                        maxScreenSize: 1024,
                                        paddingOffset: {
                                            width: 200,
                                            height: 198,
                                        },
                                    },
                                ]}
                                defaultOffset={{ width: 626, height: 164 }}
                                className="mx-auto"
                            >
                                <OvertimeAlert />
                            </ChessboardLayout>
                            <LiveChessboardProfile
                                side={ProfileSide.CurrentlyPlaying}
                            />
                        </>
                    }
                    aside={
                        <aside
                            className="grid h-full w-full min-w-xs
                                grid-rows-[minmax(100px,3fr)_100px_200px] gap-3
                                overflow-auto lg:max-w-sm"
                        >
                            <MoveHistoryTable />
                            <GameControlsCard />
                            <GameChat initialShowChat={preferences.showChat} />
                        </aside>
                    }
                />
            </ChessboardStoreContext.Provider>
        </LiveChessStoreContext.Provider>
    );
};
export default LiveChessboard;
