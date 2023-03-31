import {Button, Card, CardBody, CardHeader, Container, FormGroup, Input, InputGroup, Label} from "reactstrap";
import {useEffect, useState} from "react";

export default function Home() {
  let [posts, setPosts] = useState([]);

  useEffect(() => {
    fetch("api/Post")
      .then(r => r.json())
      .then(j => setPosts(j))
  }, [])

  return (
    <Container>
      <div>
        <InputGroup>
          <FormGroup>
            <Label for="exampleText">
              Message
            </Label>
            <Input
              id="exampleText"
              name="text"
              type="textarea"
            />
          </FormGroup>
          <Button>
            Send
          </Button>
        </InputGroup>
      </div>
      <div>
        {posts.map((item, index) => (
          <div>
            <Card>
              <CardHeader>
                {item.User.Name}
              </CardHeader>
              <CardBody>
                {item.Message}
              </CardBody>
            </Card>
          </div>
        ))}
      </div>
    </Container>
  )
}