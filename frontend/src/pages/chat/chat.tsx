import React from "react";
import { useParams } from 'react-router-dom';
import api from "../../modules/api";

import "./chat.css"
import Message_User from "./message_usr";
import Message_Agent from "./message_agent";

interface Props {
    modelId: string | undefined,

}

interface SessionInfo {
    url: string;

    activeSocket: WebSocket | undefined;

    agentStreamingMessage: string,
    messageStack: MessageEntry[],
}

export interface MessageEntry {
    personName: string | undefined,
    content: string
}


class _Chat extends React.Component<Props, SessionInfo> {
    agentStreamingMessage: string = ""
    userInputField: any;

    constructor(props: Props) {
        super(props);

        this.onOpenSocket = this.onOpenSocket.bind(this);
        this.onRecieveMessage = this.onRecieveMessage.bind(this);
        this.onCloseSocket = this.onCloseSocket.bind(this);
        this.onSocketError = this.onSocketError.bind(this);

        this.sendMessage = this.sendMessage.bind(this);

        this.userInputField = React.createRef();

        this.state = {
            url: "",

            activeSocket: undefined,

            agentStreamingMessage: "",
            messageStack: [],
        };
    }

    async componentDidMount() {
        const sessionInfo = await api.get<SessionInfo>(`ModelSocket/${this.props.modelId}/info`)

        this.setState(sessionInfo.data, () => {
            if (this.state.url == undefined || this.state.activeSocket)
                return;

            const activeSocket: WebSocket = new WebSocket(this.state.url);

            activeSocket.onopen = this.onOpenSocket;
            activeSocket.onmessage = this.onRecieveMessage;
            activeSocket.onclose = this.onCloseSocket;
            activeSocket.onerror = this.onSocketError;

            this.setState({ ...this.state, activeSocket: activeSocket });
        });
    }


    onOpenSocket() {
        console.log("Connect");
    }

    onRecieveMessage(evnt: any) {
        if (evnt.data === "[[END]]") {

            const fullMessage: MessageEntry = {
                personName: "Agent",
                content: this.agentStreamingMessage
            }

            this.setState(prev => ({
                messageStack: [...prev.messageStack, fullMessage],
                agentStreamingMessage: "",
            }));

            this.agentStreamingMessage = "";
            return;
        }

        this.agentStreamingMessage += evnt.data;
        this.setState(_ => ({ agentStreamingMessage: this.agentStreamingMessage }));
    }

    onCloseSocket() {

    }

    onSocketError(err: any) {
        console.log(err.data);
    }

    sendMessage() {
        const inp = this.userInputField.current.value; ""
        this.userInputField.current.value = "";

        const msg: MessageEntry = {
            personName: undefined,
            content: inp
        }

        this.setState(
            prev => ({
                messageStack: [...prev.messageStack, msg]
            }),
            () => {
                if (this.state.activeSocket === undefined || this.state.activeSocket.readyState !== WebSocket.OPEN)
                    return;

                this.state.activeSocket!.send(msg.content)
            }
        );
    }


    render(): React.ReactNode {
        return (
            <div className="pages_chat">
                <h1>{this.state.url}</h1>

                <div className="pages_chat_content">
                    {
                        this.state.messageStack.map(x => {

                            if (x.personName === undefined)
                                return (<Message_User personName="" content={x.content} />)

                            return <Message_Agent personName="" content={x.content} />
                        })
                    }
                    <Message_Agent personName="" content={this.state.agentStreamingMessage} />
                </div>
                <div className="pages_chat_input">
                    <input type="text" ref={this.userInputField} />
                    <button onClick={this.sendMessage}>Enter</button>
                </div>
            </div>
        )
    }
}

export function Chat() {

    const { id } = useParams<{ id: string }>();


    return <_Chat modelId={id} />
}