import React from "react";
import { useNavigate } from 'react-router-dom';

import type { ModelSession } from "../../types/models/ModelSession";

interface Props {
    model: ModelSession;
    navigate: any;
}

class _ActiveModel extends React.Component<Props, any> {
    constructor(props: any) {
        super(props);

        this.requestChatbox = this.requestChatbox.bind(this);
    }

    requestChatbox() {
        this.props.navigate(`/chat/${this.props.model.modelId}`)
    }

    render(): React.ReactNode {
        return (
            <button onClick={this.requestChatbox}>{this.props.model.modelId}</button>
        );
    }
}

export default function ActiveModel({ model }: { model: ModelSession }) {
    return <_ActiveModel model={model} navigate={useNavigate()} />
}