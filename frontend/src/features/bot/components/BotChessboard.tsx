"use client";

import { StoreApi } from "zustand";

import LiveChessboardProfile, {
    ProfileSide,
} from "@/features/liveGame/components/LiveChessboardProfile";
import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import createLiveChessStore, {
    LiveChessStore,
} from "@/features/liveGame/stores/liveChessStore";

import useEnsureLegalMovesForViewedPosition from "@/features/liveGame/hooks/useEnsureLegalMovesForViewedPosition";
import MoveHistoryTable from "@/features/chessboard/components/moveHistory/MoveHistoryTable";
import ChessboardWithSidebar from "@/features/chessboard/components/ChessboardWithSidebar";
import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import useSyncBoardInteraction from "@/features/liveGame/hooks/useSyncBoardInteraction";
import LiveChessStoreContext from "@/features/liveGame/contexts/liveChessContext";
import ChessboardLayout from "@/features/chessboard/components/ChessboardLayout";
import { ProcessedGameState } from "@/features/liveGame/lib/gameStateProcessor";
import useInvalidateOnNavigate from "@/hooks/useInvalidateOnNavigate";
import { useSessionUser } from "@/features/auth/hooks/useSessionUser";
import BotGameControlsCard from "./GameControls/BotGameControlsCard";
import processBotGameState from "../lib/botStateProcessor";
import useBotMoveEmitter from "../hooks/useBotMoveEmitter";
import useLiveBotEvents from "../hooks/useLiveBotEvents";
import BotGameOverPopup from "./BotGameOverPopup";
import { BotGameState } from "@/lib/apiClient";
import useConst from "@/hooks/useConst";
import BotDialog from "./BotDialog";

const BotChessboard = ({
    gameToken,
    gameState,
}: {
    gameToken: string;
    gameState: BotGameState;
}) => {
    const user = useSessionUser();

    const storeProps = useConst<ProcessedGameState>(() =>
        processBotGameState(gameToken, user?.userId ?? "", gameState),
    );

    const liveChessStore = useConst<StoreApi<LiveChessStore>>(() =>
        createLiveChessStore(storeProps.live),
    );

    const chessboardStore = useConst<StoreApi<ChessboardStore>>(() =>
        createChessboardStore(storeProps.board),
    );

    useEnsureLegalMovesForViewedPosition(gameState.initialFen, chessboardStore);
    useSyncBoardInteraction(liveChessStore, chessboardStore);
    useBotMoveEmitter(liveChessStore, chessboardStore);
    useLiveBotEvents(liveChessStore, chessboardStore);
    useInvalidateOnNavigate();

    return (
        <LiveChessStoreContext.Provider value={liveChessStore}>
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <BotGameOverPopup botType={gameState.botType} />
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
                            />
                            <LiveChessboardProfile
                                side={ProfileSide.CurrentlyPlaying}
                            />
                        </>
                    }
                    aside={
                        <aside
                            className="grid h-full w-full min-w-xs
                                grid-rows-[152px_minmax(100px,2fr)_100px] gap-3
                                lg:max-w-md"
                        >
                            <BotDialog
                                botColor={gameState.botColor}
                                botType={gameState.botType}
                                chessboardStore={chessboardStore}
                            />
                            <MoveHistoryTable />
                            <BotGameControlsCard botType={gameState.botType} />
                        </aside>
                    }
                />
            </ChessboardStoreContext.Provider>
        </LiveChessStoreContext.Provider>
    );
};
export default BotChessboard;
