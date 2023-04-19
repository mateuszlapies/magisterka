import {useEffect, useState} from "react";
import {
    Button,
    Form,
    FormFeedback,
    FormGroup,
    FormText,
    Input,
    Label,
    Modal,
    ModalBody,
    ModalHeader,
    Spinner
} from "reactstrap";

export default function Identity() {
    let [hasUser, setHasUser] = useState(false);
    let [valid, setValid] = useState();
    let [invalid, setInvalid] = useState();
    let [processing, setProcessing] = useState(false);
    let [job, setJob] = useState();

    useEffect(() => {
        if (!processing) {
            fetch("api/Status/User")
              .then(r => r.json())
              .then(j => setHasUser(j.object));
        }
    }, [processing]);

    useEffect(() => {
        if (job) {
            let interval = setInterval(() => {
                fetch("api/Status/Job?id=" + job)
                  .then(r => r.json())
                  .then(j => {
                      if (j.object === "Succeeded") {
                          setJob(undefined)
                          setProcessing(false);
                          clearInterval(interval);
                      }
                  });
            }, 1000);
        }
    }, [job]);

    let onChange = (e) => {
        fetch("api/Status/Username?username=" + e.target.value)
            .then(r => r.json())
            .then(j => {
                if (j.object) {
                    setValid(true);
                    setInvalid(false);
                } else {
                    setValid(false);
                    setInvalid(true);
                }
            })
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
            .then(j => setJob(j.object));
    }

    let submit = (p) => {
        if (p) {
            return <Spinner size="sm">Loading...</Spinner>
        } else {
            return 'Submit'
        }
    }
    return (
        <Modal centered isOpen={!hasUser}>
            <ModalHeader className="text-center d-block">
                Register
            </ModalHeader>
            <ModalBody>
                <Form onSubmit={onSubmit}>
                    <FormGroup>
                        <Label for="username">
                            Username
                        </Label>
                        <Input id="username" valid={valid} invalid={invalid} onChange={onChange} />
                        <FormFeedback valid>
                            This username is available
                        </FormFeedback>
                        <FormFeedback>
                            This username is not available
                        </FormFeedback>
                        <FormText>
                            Pick your username. You can change it later.
                        </FormText>
                    </FormGroup>
                    <Button className="float-end" type="submit" disabled={!valid || processing}>
                        {submit(processing)}
                    </Button>
                </Form>
            </ModalBody>
        </Modal>
    );
}