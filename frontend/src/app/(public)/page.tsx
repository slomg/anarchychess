import { Metadata } from "next";

import HomeDiscordSection from "@/features/home/components/HomeDiscordSection";
import HomeIntroSection from "@/features/home/components/HomeIntroSection";
import HomeFooter from "@/features/home/components/HomeFooter";
import HomeHero from "@/features/home/components/HomeHero";

export const metadata: Metadata = {
    title: "Anarchy Chess",
    description:
        "Play Anarchy Chess online with wild, custom rules and unique pieces inspired by r/AnarchyChess." +
        "Explore chaotic chess variants, challenge friends, complete quests, climb the ranks, and experience the unpredictable fun of anarchychess gameplay.",
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
            <HomeIntroSection />
            <HomeDiscordSection />
            <HomeFooter />
        </main>
    );
}
export default HomePage;
