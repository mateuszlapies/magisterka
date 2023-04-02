import {Progress} from "reactstrap";
import {useEffect} from "react";

export default function Sync() {
  useEffect(() => {
    let interval = setInterval(() => {
      fetch("/api/Status/Synced")
        .then(r => r.json())
        .then(t => {
          if (t.object === true) {
            clearInterval(interval)
            window.location.href = "app"
          }
        })
    }, 10000)
  }, [])

  return (
    <div className="d-table full">
      <div className="d-table-cell full-height middle text-center">
        <p className="h1 m-3">Synchronizing</p>
        <Progress
          animated
          color="info"
          value={100}
        />
      </div>
    </div>
  )
}