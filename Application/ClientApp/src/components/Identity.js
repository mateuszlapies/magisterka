import {useEffect, useState} from "react";
import {Button, Form, FormFeedback, FormGroup, FormText, Input, Label, Modal, ModalBody, ModalHeader} from "reactstrap";

export default function Identity() {
    let [hasUser, setHasUser] = useState(false);
    let [valid, setValid] = useState();
    let [processing, setProcessing] = useState(false);

    useEffect(() => {
        fetch("api/Status/User")
            .then(r => r.json())
            .then(j => setHasUser(j.object));
    });

    let onChange = (e) => {
        fetch("api/Status/Username?name=" + e.target.value)
            .then(r => r.json())
            .then(j => setValid(j.object))
    }

    let onSubmit = (e) => {
        e.preventDefault();
        setProcessing(true);
        fetch("api/User/CreateUser", {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ object: e.target.username.value })
        })
            .then(r => r.json())
            .then(j => {
                console.log(j)
                setProcessing(false);
            });
    }

    return (
        <Modal isOpen={hasUser}>
            <ModalHeader>
                Register
            </ModalHeader>
            <ModalBody>
                <Form onSubmit={onSubmit}>
                    <FormGroup>
                        <Label for="username">
                            Username
                        </Label>
                        <Input id="username" valid={valid} onChange={onChange} />
                        <FormFeedback valid={true}>
                            This username is available
                        </FormFeedback>
                        <FormFeedback valid={false}>
                            This username is not available
                        </FormFeedback>
                        <FormText>
                            Pick your username. You can change it later.
                        </FormText>
                    </FormGroup>
                    <Button type="submit" disabled={!valid || processing}>
                        Submit
                    </Button>
                </Form>
            </ModalBody>
        </Modal>
    );
}