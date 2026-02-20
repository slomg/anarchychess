import BotPlayOptions from "@/features/bot/components/BotPlayOptions";
import StaticChessboardWithSidebar from "@/features/chessboard/components/StaticChessboardWithSidebar";

export default async function BotPage() {
    return (
        <StaticChessboardWithSidebar
            prioritizeAside
            aside={<BotPlayOptions />}
        />
    );
}
