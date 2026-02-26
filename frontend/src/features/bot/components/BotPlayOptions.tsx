"use client";

import { useState } from "react";
import clsx from "clsx";

import Selector from "@/components/ui/Selector";
import useBotMatch from "../hooks/useBotMatch";
import Button from "@/components/ui/Button";
import { GameColor } from "@/lib/apiClient";
import Card from "@/components/ui/Card";
import ProfilePicture from "@/features/profile/components/ProfilePicture";

const BotPlayOptions = () => {
    const [selected, setSelected] = useState("bot:anarchybot");
    const [color, setColor] = useState<GameColor | null>(null);
    const [error, setError] = useState<string>();

    const { matchBotGame, isMatching } = useBotMatch();

    async function startGame() {
        setError(undefined);

        const success = await matchBotGame(color);
        if (!success) {
            setError("Failed to start game. Please try again.");
            return;
        }
    }

    return (
        <Card
            className="relative h-full w-full min-w-xs flex-col gap-3
                overflow-auto lg:max-w-sm"
        >
            <div className="flex flex-wrap gap-4 text-center">
                <Bot
                    label="Anarchy Bot"
                    userId="bot:anarchybot"
                    selected={selected}
                    select={() => setSelected("bot:anarchybot")}
                />

                <Bot
                    label="Lobotomized Anarchy Bot"
                    userId="bot:lobotomized-anarchybot"
                    selected={selected}
                    disabled
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
                            label: <span className="text-white">♚</span>,
                            value: GameColor.WHITE,
                        },
                        { label: "◐", value: null },
                        {
                            label: <span className="text-black">♚</span>,
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
    selected: string;
    select?: () => void;
    disabled?: boolean;
}) => {
    return (
        <div
            className={clsx(
                `outline-accent flex h-min w-min flex-col items-center gap-1
                rounded-xl p-2`,
                selected === userId && "outline-4",
                disabled &&
                    "cursor-not-allowed brightness-75 grayscale select-none",
                !disabled && "cursor-pointer",
            )}
            onClick={select}
        >
            <ProfilePicture userId={userId} size={100} />
            <p className="text-balance">{label}</p>
        </div>
    );
};
