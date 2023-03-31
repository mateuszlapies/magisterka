import {Button, Card, CardBody, CardHeader, FormGroup, Input, InputGroup, Label} from "reactstrap";
import {useEffect, useState} from "react";

export default function Home() {
  let [posts, setPosts] = useState([]);

  useEffect(() => {
    fetch("api/Post")
      .then(r => r.json())
      .then(j => setPosts(j))
  }, [])

  return (
    <>
      <div className="p-3">
        <InputGroup>
          <Input
            name="text"
            type="textarea"
          />
          <Button>
            <i className="bi bi-send"/>
          </Button>
        </InputGroup>
      </div>
      <div>
        {posts.map((item, index) => (
          <div key={index} className="m-3">
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
    </>
  )
}