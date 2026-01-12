import AnalysisChessboard from "@/features/analysis/components/AnalysisChessboard";
import { getInitialAnalysisPosition } from "@/lib/apiClient";
import dataOrThrow from "@/lib/apiClient/dataOrThrow";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "Analysis - Anarchy Chess",
    description: "Analyze Anarchy Chess with new rules and custom pieces.",
    keywords: [
        "anarchy chess analysis",
        "chess variants analysis",
        "custom chess analysis",
        "singleplayer chess analysis",
    ],
};

export default async function AnalysisPage() {
    const rootPosition = await dataOrThrow(getInitialAnalysisPosition());
    return <AnalysisChessboard rootPosition={rootPosition} />;
}
