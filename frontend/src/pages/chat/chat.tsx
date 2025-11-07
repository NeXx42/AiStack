import React from "react";
import { useParams } from 'react-router-dom';

interface Props {
    modelId: string | undefined
}


class _Chat extends React.Component<Props, any> {
    render(): React.ReactNode {
        return (
            <div className="pages_chat">
                <h1>{this.props.modelId}</h1>

                <div className="pages_chat_content">
                    <p />
                </div>
                <div className="pages_chat_input">
                    <input></input>
                </div>
            </div>
        )
    }
}

export function Chat() {
    const { id } = useParams<{ id: string }>();
    return <_Chat modelId={id} />
}