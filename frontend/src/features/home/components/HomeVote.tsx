import VoteView from "@/features/vote/components/VoteView";

const HomeVote = () => {
    return (
        <section className="bg-background p-5 lg:p-15">
            <div className="mx-auto flex max-w-5xl flex-col gap-5 p-6">
                <h1
                    className="text-center text-4xl text-balance sm:text-start
                        sm:text-5xl"
                >
                    Would You Rather
                </h1>

                <VoteView />
            </div>
        </section>
    );
};
export default HomeVote;
