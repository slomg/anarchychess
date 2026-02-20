"use client";

import Image, { StaticImageData } from "next/image";
import { useRouter } from "next/navigation";
import { useState } from "react";
import clsx from "clsx";

import AnarchyBotPfp from "@public/assets/bots/anarchybot.png";
import { GameColor, startBotGame } from "@/lib/apiClient";
import Selector from "@/components/ui/Selector";
import Button from "@/components/ui/Button";
import Card from "@/components/ui/Card";
import constants from "@/lib/constants";
import { randomizeColor } from "@/lib/utils/chessUtils";

const BotPlayOptions = () => {
    const [selected, setSelected] = useState("Anarchy Bot");
    const [color, setColor] = useState<GameColor | null>(null);
    const [isFetching, setIsFetching] = useState(false);
    const [error, setError] = useState<string>();

    const router = useRouter();

    async function startGame() {
        setError(undefined);
        setIsFetching(true);

        try {
            const myColor = color ?? randomizeColor();
            const { error, data: gameToken } = await startBotGame({
                query: { myColor },
            });

            if (error || !gameToken) {
                setError("Failed to start game. Please try again.");
                console.error("BotPlayOptions startGame", error);
                return;
            }

            router.push(`${constants.PATHS.BOT}/${gameToken}`);
        } finally {
            setIsFetching(false);
        }
    }

    return (
        <Card
            className="relative h-full w-full min-w-xs flex-col gap-3
                overflow-auto lg:max-w-sm"
        >
            <div
                className="flex gap-4 text-center text-nowrap lg:grid
                    lg:grid-cols-2"
            >
                <Bot
                    name="Anarchy Bot"
                    profilePicture={AnarchyBotPfp}
                    selected={selected}
                    select={() => setSelected("Anarchy Bot")}
                />

                <Bot
                    name="Coming Soon™"
                    profilePicture={AnarchyBotPfp}
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
                    disabled={isFetching}
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
    name,
    profilePicture,
    selected,
    select,
    disabled = false,
}: {
    name: string;
    profilePicture: StaticImageData;
    selected: string;
    select?: () => void;
    disabled?: boolean;
}) => {
    return (
        <div
            className={clsx(
                "outline-accent flex flex-col items-center gap-1 rounded-xl p-2",
                selected === name && "outline-4",
                disabled &&
                    "cursor-not-allowed brightness-75 grayscale select-none",
                !disabled && "cursor-pointer",
            )}
            onClick={select}
        >
            <Image
                src={profilePicture}
                className="rounded-md"
                width={150}
                height={150}
                alt={name}
                draggable={false}
            />
            <p>{name}</p>
        </div>
    );
};
