import {Button, Card, CardBody, CardHeader, Form, Input, InputGroup} from "reactstrap";
import {useEffect, useState} from "react";

export default function Home() {
  let [posts, setPosts] = useState([]);

  useEffect(() => {
    fetch("api/Post/GetPosts")
      .then(r => r.json())
      .then(j => setPosts(j.object));
  }, [])

  let onSubmit = (e) => {
    e.preventDefault();
    fetch("api/Post/CreatePost", {
      method: "PUT",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({ object: e.target.message.value })
    })
      .then(r => r.json())
      .then(j => console.log(j));
  }

  return (
    <>
      <div className="p-3">
        <Form onSubmit={onSubmit}>
          <InputGroup>
            <Input
              name="message"
              type="textarea"
            />
            <Button type="submit">
              <i className="bi bi-send"/>
            </Button>
          </InputGroup>
        </Form>
      </div>
      <div>
        {posts.map((item, index) => (
          <div key={index} className="m-3">
            <Card>
              <CardHeader>
                {item.value.name}
              </CardHeader>
              <CardBody>
                {item.key.message}
              </CardBody>
            </Card>
          </div>
        ))}
      </div>
    </>
  )
}