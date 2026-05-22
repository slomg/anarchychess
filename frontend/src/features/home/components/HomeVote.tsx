import VoteView from "@/features/vote/components/VoteView";

const HomeVote = () => {
    return (
        <section
            className="bg-[#0b131a] p-5 lg:p-15"
            id="vote"
            data-testid="homeVote"
        >
            <div className="mx-auto flex max-w-5xl flex-col gap-5 p-6">
                <div className="text-center sm:text-start">
                    <h1 className="text-3xl text-balance sm:text-5xl">
                        Would You Rather
                    </h1>
                    <h2 className="text-text/70">
                        Ideas from the Discord. Vote for your favorites.
                    </h2>
                </div>

                <VoteView />
            </div>
        </section>
    );
};
export default HomeVote;
