import Sync from "./components/Sync";
import Home from "./components/Home";

const AppRoutes = [
  {
    index: true,
    element: <Sync />
  },
  {
    path: '/app',
    element: <Home />
  }
];

export default AppRoutes;
