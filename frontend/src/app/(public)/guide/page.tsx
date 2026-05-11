import GuideSide from "@/features/guide/components/GuideSide";
import NewPiecesGuide from "@/features/guide/components/NewPiecesGuide";
import NewRulesGuide from "@/features/guide/components/NewRulesGuide";

export const metadata = { title: "Guide - Anarchy Chess" };

export default function GuidePage() {
    return (
        <main className="flex flex-1 justify-center gap-3 p-5">
            <GuideSide piecesGuideHref="#pieces" rulesGuideHref="#rules" />
            <div className="flex w-full max-w-7xl flex-1 flex-col gap-5">
                <NewPiecesGuide id="pieces" />
                <NewRulesGuide id="rules" />
            </div>
        </main>
    );
}
