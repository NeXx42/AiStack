import React from "react";
import api from "../../modules/api";

import "./home.css"
import type { ModelSession } from "../../types/models/ModelSession";
import ActiveModel from "./activeModel";

interface State {
    modelTypes: string[];
    activeModels: ModelSession[];

    selectedModel: string | undefined;
}

class Home extends React.Component<any, State> {
    constructor(props: any) {
        super(props);

        this.state = {
            modelTypes: [],
            activeModels: [],

            selectedModel: undefined
        }

        this.createModel = this.createModel.bind(this);
        this.updateActiveModels = this.updateActiveModels.bind(this);
        this.changeSelectedModel = this.changeSelectedModel.bind(this);
    }

    async componentDidMount() {
        await this.updateActiveModels();
        var modelTypes = await api.get("/Models/types");

        this.setState({
            modelTypes: modelTypes.data,
        });
    }

    async createModel() {
        console.log(`creating model - ${this.state.selectedModel}`);
        if (this.state.selectedModel === undefined)
            return;

        await api.post(`/Models/${this.state.selectedModel}/start`);
        await this.updateActiveModels();
    }

    async updateActiveModels() {
        var activeModels = await api.get<ModelSession[]>("Models/active");

        this.setState({
            activeModels: activeModels.data
        });
    }

    changeSelectedModel(event: React.ChangeEvent<HTMLSelectElement>) {
        this.setState({ selectedModel: event.target.value });
    }

    render() {
        return (
            <div className="pages_home">
                <div className="pages_home_creation">
                    <h1>Test</h1>

                    <select onChange={this.changeSelectedModel}>
                        <option></option>
                        {
                            this.state.modelTypes.map(x => {
                                return (<option value={x}>{x}</option>);
                            })
                        }
                    </select>

                    <button onClick={this.createModel}>Create</button>
                </div>


                <div className="pages_home_active">
                    {
                        this.state.activeModels.map(x => {
                            return (<ActiveModel model={x} />)
                        })
                    }
                </div>
            </div>
        );
    }
}

export default Home;