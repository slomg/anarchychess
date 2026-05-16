"use client";

import { useCallback, useEffect, useState } from "react";
import clsx from "clsx";

import {
    completeVote,
    getNextVotePair,
    PendingUserVote,
    VoteOption,
} from "@/lib/apiClient";

import VoteCard from "@/features/vote/components/VoteCard";
import Button from "@/components/ui/Button";

enum VoteOptionType {
    OPTION_A,
    OPTION_B,
}

export const VOTE_DELAY_MS = 1000;

const VoteView = () => {
    const [votePair, setVotePair] = useState<
        PendingUserVote | null | undefined
    >();
    const [selectedOptionType, setSelectedOptionType] =
        useState<VoteOptionType | null>(null);

    const [error, setError] = useState<string | null>(null);
    const [isFetching, setIsFetching] = useState(false);
    const [isHolding, setIsHolding] = useState(false);

    useEffect(() => {
        if (votePair === undefined) {
            setNextVotePair();
        }
    }, [votePair]);
    let selectedOption: VoteOption | null = null;
    if (votePair != null && selectedOptionType !== null) {
        selectedOption =
            selectedOptionType === VoteOptionType.OPTION_A
                ? votePair.optionA
                : votePair.optionB;
    }

    function selectOption(option: VoteOptionType) {
        if (!isFetching) {
            setSelectedOptionType(option);
        }
    }

    async function setNextVotePair() {
        const { error, data } = await getNextVotePair();
        if (error && error.status !== 404) {
            console.error(
                "HomeVote loadPendingVoteEvent getNextVotePair",
                error,
            );
            return;
        }

        if (!data) {
            setVotePair(null);
            return;
        }

        setVotePair(data);
        setSelectedOptionType(null);
    }

    function startHold() {
        if (!selectedOption || isFetching) {
            return;
        }

        setIsHolding(true);
        document.addEventListener("pointerup", cancelHold);
        document.addEventListener("pointercancel", cancelHold);
    }

    const cancelHold = useCallback(() => {
        document.removeEventListener("pointerup", cancelHold);
        document.removeEventListener("pointercancel", cancelHold);
        setIsHolding(false);
    }, []);

    async function handleHoldTransitionEnd(event: React.TransitionEvent) {
        if (event.propertyName === "width" && isHolding) {
            document.removeEventListener("pointerup", cancelHold);
            document.removeEventListener("pointercancel", cancelHold);
            await castVote();
        }
    }

    async function castVote() {
        if (!selectedOption) {
            return;
        }

        setError(null);
        setIsFetching(true);
        try {
            const [{ response, error }] = await Promise.all([
                completeVote({
                    query: { optionKey: selectedOption.optionKey },
                }),
                new Promise((r) => setTimeout(r, VOTE_DELAY_MS)),
            ]);

            if (response.status === 429) {
                setError("You are being rate limited. Please try again later.");
                return;
            } else if (error) {
                console.error("HomeVote castVote completeVote", error);
                setError("Something went wrong.");
                return;
            }

            await setNextVotePair();
        } finally {
            setIsFetching(false);
            setIsHolding(false);
        }
    }

    if (votePair === undefined) {
        return null;
    }

    if (votePair === null) {
        return (
            <p>
                There&apos;s nothing left for you to vote for! Come back later
                to check if new options are added.
            </p>
        );
    }

    return (
        <>
            <section className="flex w-full flex-col gap-5 md:flex-row">
                <VoteCard
                    option={votePair.optionA}
                    optionLetter="A"
                    isSelected={selectedOptionType === VoteOptionType.OPTION_A}
                    onClick={() => selectOption(VoteOptionType.OPTION_A)}
                />
                <VoteCard
                    option={votePair.optionB}
                    optionLetter="B"
                    isSelected={selectedOptionType === VoteOptionType.OPTION_B}
                    onClick={() => selectOption(VoteOptionType.OPTION_B)}
                />
            </section>

            <Button
                className={clsx(
                    "bg-card relative text-2xl select-none",
                    isHolding && "cursor-grabbing",
                )}
                disabled={selectedOptionType === null || isFetching}
                onPointerDown={startHold}
            >
                {selectedOption ? (
                    <>
                        LONG PRESS TO CAST VOTE FOR{" "}
                        <span className="text-accent">
                            {selectedOption.name.toUpperCase()}
                        </span>
                    </>
                ) : (
                    "SELECT A RULE TO VOTE"
                )}

                <div
                    className={clsx(
                        "bg-primary/60 absolute top-0 left-0 h-full rounded-md",
                        isHolding ? "w-full" : "w-0",
                        selectedOptionType !== null &&
                            "transition-[width] duration-4000 ease-in",
                    )}
                    onTransitionEnd={handleHoldTransitionEnd}
                    data-testid="voteViewHoldProgress"
                />
            </Button>
            {error && <span className="text-error">{error}</span>}
        </>
    );
};
export default VoteView;
