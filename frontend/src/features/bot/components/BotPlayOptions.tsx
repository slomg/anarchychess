"use client";

import { useState } from "react";
import clsx from "clsx";

import ProfilePicture from "@/features/profile/components/ProfilePicture";
import traitorRook from "@public/assets/pieces/traitor_rook_neutral.png";
import whiteKing from "@public/assets/pieces/king_white.png";
import blackKing from "@public/assets/pieces/king_black.png";
import { BotType, GameColor } from "@/lib/apiClient";
import useLocalPref from "@/hooks/useLocalPref";
import Selector from "@/components/ui/Selector";
import useBotMatch from "../hooks/useBotMatch";
import Button from "@/components/ui/Button";
import Card from "@/components/ui/Card";
import constants from "@/lib/constants";
import Image from "next/image";

const BotPlayOptions = () => {
    const [selectedBotType, setSelectedBotType] = useLocalPref<BotType>(
        constants.LOCALSTORAGE.PREFERS_BOT_TYPE,
        BotType.ANARCHY_BOT,
    );
    const [color, setColor] = useState<GameColor | null>(null);
    const [error, setError] = useState<string>();

    const { matchBotGame, isMatching } = useBotMatch();

    async function startGame() {
        setError(undefined);

        const success = await matchBotGame(color, selectedBotType);
        if (!success) {
            setError("Failed to start game. Please try again.");
            return;
        }
    }

    return (
        <Card
            className="relative h-full w-full min-w-xs flex-col gap-5
                overflow-auto lg:max-w-sm"
        >
            <div className="flex flex-wrap gap-4">
                <Bot
                    label="Anarchy Bot"
                    userId="bot:anarchybot"
                    selected={selectedBotType === BotType.ANARCHY_BOT}
                    select={() => setSelectedBotType(BotType.ANARCHY_BOT)}
                />
                <Bot
                    label="Lobotomized Anarchy Bot"
                    userId="bot:lobotomized-anarchybot"
                    selected={
                        selectedBotType === BotType.LOBOTOMIZED_ANARCHY_BOT
                    }
                    select={() =>
                        setSelectedBotType(BotType.LOBOTOMIZED_ANARCHY_BOT)
                    }
                />
            </div>

            <div className="flex flex-1 flex-col justify-end gap-3 text-3xl">
                <div
                    tabIndex={0}
                    className="group relative inline-flex items-center text-sm"
                >
                    <span className="text-text/70 cursor-help">
                        Bots don&apos;t throw pawns yet.{" "}
                        <span className="underline">(details)</span>
                    </span>

                    <div
                        className="pointer-events-none absolute bottom-full
                            left-0 z-50 mb-2 w-64 rounded-md bg-black/90 p-2
                            text-xs opacity-0 transition
                            group-focus-within:opacity-100
                            group-hover:opacity-100"
                    >
                        <p>
                            Pawn Throwing is not supported in bot evaluation
                            yet. It significantly increases the number of legal
                            moves (~4x), and I am not sure how to handle that
                            yet. It&apos;s planned for a future update!
                        </p>

                        <p className="mt-4">
                            You can still play pawn throw moves against the bot,
                            but it will not consider throw moves or stunned
                            pieces when evaluating positions.
                        </p>
                    </div>
                </div>

                {error && (
                    <p
                        className="text-error text-lg"
                        data-testid="botPlayOptionsError"
                    >
                        {error}
                    </p>
                )}

                <Selector
                    options={[
                        {
                            label: (
                                <Image
                                    src={whiteKing}
                                    alt="play as white"
                                    className="mx-auto"
                                    width={50}
                                    height={50}
                                />
                            ),
                            value: GameColor.WHITE,
                        },
                        {
                            label: (
                                <Image
                                    src={traitorRook}
                                    alt="random color"
                                    className="mx-auto"
                                    width={50}
                                    height={50}
                                />
                            ),
                            value: null,
                        },
                        {
                            label: (
                                <Image
                                    src={blackKing}
                                    alt="play as black"
                                    className="mx-auto"
                                    width={50}
                                    height={50}
                                />
                            ),
                            value: GameColor.BLACK,
                        },
                    ]}
                    value={color}
                    onChange={(color) => setColor(color.target.value)}
                    data-testid="botPlayOptionsColorSelector"
                />
                <Button
                    className="h-min"
                    onClick={startGame}
                    disabled={isMatching}
                    data-testid="botPlayOptionsStartButton"
                >
                    Play
                </Button>
            </div>
        </Card>
    );
};
export default BotPlayOptions;

const Bot = ({
    label,
    userId,
    selected,
    select,
    disabled = false,
}: {
    label: string;
    userId: string;
    selected: boolean;
    select?: () => void;
    disabled?: boolean;
}) => {
    return (
        <div
            className={clsx(
                "flex h-min w-min flex-col items-center gap-1 rounded-xl",
                disabled &&
                    "cursor-not-allowed brightness-75 grayscale select-none",
                !disabled && "cursor-pointer",
            )}
            onClick={select}
        >
            <ProfilePicture
                className={clsx(
                    selected && "border-secondary rounded-xl border-3 p-1",
                )}
                userId={userId}
                size={104}
            />
            <p className="text-center text-balance">{label}</p>
        </div>
    );
};
