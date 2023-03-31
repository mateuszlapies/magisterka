import {Progress} from "reactstrap";
import {useEffect} from "react";

export default function Sync() {
  useEffect(() => {
    setTimeout(() => {
      fetch("/api/Status/Synced")
        .then(r => r.text())
        .then(t => t === "true" ? window.href = "app" : null)
    }, 1000)
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