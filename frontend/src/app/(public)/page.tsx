import { Metadata } from "next";

import HomeDiscordSection from "@/features/home/components/HomeDiscordSection";
import HomeIntroSection from "@/features/home/components/HomeIntroSection";
import HomeFooter from "@/features/home/components/HomeFooter";
import HomeHero from "@/features/home/components/HomeHero";
import HomeVote from "@/features/home/components/HomeVote";

export const metadata: Metadata = {
    title: "Anarchy Chess",
    description:
        "Anarchy Chess is a chaotic chess variant with unique pieces and custom rules inspired by r/AnarchyChess. " +
        "Play online, experiment with unusual mechanics, face off against bots, complete quests and more.",
    keywords: [
        "chess",
        "anarchy chess",
        "custom chess rules",
        "online chess",
        "chess variants",
        "reddit chess",
    ],
};

async function HomePage() {
    return (
        <main
            className="grid flex-1 grid-cols-1
                grid-rows-[min-content_min-content_min-content_1fr]"
        >
            <HomeHero />
            <HomeDiscordSection />
            <HomeVote />
            <HomeIntroSection />
            <HomeFooter />
        </main>
    );
}
export default HomePage;
