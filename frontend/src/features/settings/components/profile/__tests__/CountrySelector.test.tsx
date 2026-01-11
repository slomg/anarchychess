import { render, screen } from "@testing-library/react";
import { Form, Formik } from "formik";

import countries from "@public/data/countries.json";
import userEvent from "@testing-library/user-event";
import CountrySelector from "../CountrySelector";

describe("CountrySelector", () => {
    const renderWithFormik = (initialValues = { country: "" }) =>
        render(
            <Formik initialValues={initialValues} onSubmit={() => {}}>
                <Form>
                    <CountrySelector name="country" />
                </Form>
            </Formik>,
        );

    it("should render a select field with the correct name inside Formik", () => {
        renderWithFormik();

        const select = screen.getByTestId("countrySelector");
        expect(select).toBeInTheDocument();
        expect(select).toHaveAttribute("name", "country");
    });

    it("should render options with the correct classes", () => {
        renderWithFormik();

        const select = screen.getByTestId("countrySelector");

        const firstOption = select.querySelector("option");
        expect(firstOption).toHaveClass("bg-white");
        expect(firstOption).toHaveClass("text-black");
    });

    it("should update Formik state when a country is selected", async () => {
        let selectedValue = "";
        const user = userEvent.setup();
        render(
            <Formik
                initialValues={{ country: "" }}
                onSubmit={(values) => {
                    selectedValue = values.country;
                }}
            >
                <Form>
                    <CountrySelector name="country" />
                    <button type="submit">Submit</button>
                </Form>
            </Formik>,
        );

        const select = screen.getByRole("combobox");
        await user.selectOptions(select, Object.keys(countries)[0]);
        expect(select).toHaveValue(Object.keys(countries)[0]);

        await user.click(screen.getByRole("button"));
        expect(selectedValue).toBe(Object.keys(countries)[0]);
    });
});
