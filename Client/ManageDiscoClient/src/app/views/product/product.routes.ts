import { RouterModule } from "@angular/router";
import { ProductComponent } from "./product.component";

export const ProductsRoutes = RouterModule.forChild([
  {path: '', component: ProductComponent}
])
