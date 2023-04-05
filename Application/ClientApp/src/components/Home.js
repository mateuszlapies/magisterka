import {Button, Card, CardBody, CardHeader, Form, Input, InputGroup, Spinner} from "reactstrap";
import {useEffect, useState} from "react";
import Identity from "./Identity";

export default function Home() {
  let [posts, setPosts] = useState([]);
  let [processing, setProcessing] = useState(false);
  let [job, setJob] = useState();

  useEffect(() => {
    if (!processing) {
      fetch("api/Post/GetPosts")
        .then(r => r.json())
        .then(j => setPosts(j.object));
    }
  }, [processing])

  useEffect(() => {
    if (job) {
      let interval = setInterval(() => {
        fetch("api/Status/Job?id=" + job)
          .then(r => r.json())
          .then(j => {
            if (j.object === "Succeeded") {
              setJob(undefined);
              setProcessing(false);
              clearInterval(interval);
            }
          })
      }, 1000);
    }
  }, [job]);

  let onSubmit = (e) => {
    e.preventDefault();
    setProcessing(true);
    fetch("api/Post/CreatePost", {
      method: "PUT",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({ object: e.target.message.value })
    })
      .then(r => r.json())
      .then(j => setJob(j.object))
      .finally(() => e.target.reset());
  }

  let icon = (p) => {
    if (p) {
      return (<Spinner size="sm">Loading...</Spinner>)
    } else {
      return (<i className="bi bi-send"/>);
    }
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
            <Button type="submit" disabled={processing}>
              {icon(processing)}
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
      <Identity/>
    </>
  )
}