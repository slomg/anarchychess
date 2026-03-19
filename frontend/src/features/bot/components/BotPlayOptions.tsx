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
