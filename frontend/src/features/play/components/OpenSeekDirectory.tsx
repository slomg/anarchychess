"use client";

import { CSSTransition, TransitionGroup } from "react-transition-group";
import React, { useEffect, useRef, useState } from "react";

import {
    useOpenSeekEmitter,
    useOpenSeekEvent,
} from "@/features/lobby/hooks/useOpenSeekHub";

import { OpenSeekToKeyStr } from "@/features/lobby/lib/matchmakingKeys";
import useLobbyStore from "@/features/lobby/stores/lobbyStore";
import { SeekKeyStr } from "@/features/lobby/lib/types";
import OpenSeekItem from "./OpenSeekItem";
import Card from "@/components/ui/Card";
import constants from "@/lib/constants";

const OpenSeekDirectory = () => {
    const { addOpenSeeks, removeOpenSeek } = useLobbyStore((x) => ({
        addOpenSeeks: x.addOpenSeeks,
        removeOpenSeek: x.removeOpenSeek,
    }));
    const openSeeks = useLobbyStore(
        (x) => x.openSeekTracker.interleavedOpenSeeks,
    );
    const [nodeRefs, setNodeRefs] = useState<
        Record<SeekKeyStr, React.RefObject<HTMLDivElement | null>>
    >({});

    const noSeeksRef = useRef<HTMLParagraphElement | null>(null);
    const [showNoSeeksText, setShowNoSeeksText] = useState(true);
    const sendOpenSeekEvent = useOpenSeekEmitter();

    useEffect(() => {
        const interval = setInterval(
            () => sendOpenSeekEvent("SubscribeAsync"),
            constants.OPEN_SEEK_RESUBSCRIBE_INTERAVAL_MS,
        );
        sendOpenSeekEvent("SubscribeAsync");
        return () => clearInterval(interval);
    }, [sendOpenSeekEvent]);

    useOpenSeekEvent("NewOpenSeeksAsync", (newOpenSeeks) => {
        addOpenSeeks(newOpenSeeks);

        setNodeRefs((prev) => {
            const copy = { ...prev };
            for (const seek of newOpenSeeks) {
                const key = OpenSeekToKeyStr(seek.userId, seek.pool);
                const ref = React.createRef<HTMLDivElement>();
                copy[key] = ref;
            }
            setShowNoSeeksText(false);

            return copy;
        });
    });

    useOpenSeekEvent("OpenSeekEndedAsync", (userId, pool) => {
        removeOpenSeek(userId, pool);

        setNodeRefs((prev) => {
            const copy = { ...prev };
            const key = OpenSeekToKeyStr(userId, pool);
            delete copy[key];

            if (Object.keys(copy).length === 0) {
                setTimeout(() => setShowNoSeeksText(true), 300);
            }

            return copy;
        });
    });

    return (
        <Card className="min-h-60 flex-1">
            <h2 className="text-center text-3xl">Open Challenges</h2>

            <div
                className="flex h-full max-h-100 flex-col gap-3 overflow-auto
                    lg:max-h-none lg:overflow-visible"
            >
                <CSSTransition
                    in={showNoSeeksText}
                    timeout={{ enter: 200, exit: 0 }}
                    classNames={{
                        enter: "opacity-0",
                        enterActive: "opacity-100 transition-all duration-200",
                    }}
                    nodeRef={noSeeksRef}
                    unmountOnExit
                >
                    <p
                        data-testid="noOpenChallengesText"
                        className="mt-4 text-center text-gray-500"
                        ref={noSeeksRef}
                    >
                        No open challenges, join a pool to appear here for
                        others
                    </p>
                </CSSTransition>

                <TransitionGroup duration={300}>
                    {openSeeks.map((seek) => {
                        const key = OpenSeekToKeyStr(seek.userId, seek.pool);
                        const nodeRef = nodeRefs[key];
                        if (!nodeRef) return;

                        return (
                            <CSSTransition
                                key={key}
                                classNames={{
                                    enter: "opacity-0 -translate-x-10",
                                    enterActive:
                                        "opacity-100 translate-x-0 transition-all duration-300",
                                    exitActive:
                                        "opacity-0 -translate-x-10 transition-all duration-300",
                                }}
                                timeout={300}
                                nodeRef={nodeRef}
                            >
                                <div ref={nodeRef}>
                                    <OpenSeekItem seek={seek} />
                                </div>
                            </CSSTransition>
                        );
                    })}
                </TransitionGroup>
            </div>
        </Card>
    );
};
export default OpenSeekDirectory;
