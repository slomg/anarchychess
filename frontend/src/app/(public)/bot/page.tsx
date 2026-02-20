import { Metadata } from "next";

import StaticChessboardWithSidebar from "@/features/chessboard/components/StaticChessboardWithSidebar";
import BotPlayOptions from "@/features/bot/components/BotPlayOptions";

export const metadata: Metadata = {
    title: "Play Bot - Anarchy Chess",
    description:
        "Challenge the Anarchy Bot in Anarchy Chess, play anarchy chess with chaotic rules and custom pieces.",
    keywords: [
        "play bot chess",
        "chess variants",
        "anarchy chess bot",
        "chess engine",
        "play against bot",
        "anarchy chess",
    ],
};

export default async function BotPage() {
    return (
        <StaticChessboardWithSidebar
            prioritizeAside
            aside={<BotPlayOptions />}
        />
    );
}
