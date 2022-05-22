import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule, Routes } from '@angular/router';
import { AppComponent } from './app.component';
import {HttpClientModule, HTTP_INTERCEPTORS} from '@angular/common/http'
import { ApiCaller } from './api/api';
import { Endpoints } from './api/url';
import { ApiHttpService } from './api/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ApiInterceptor } from './api/ApiInterceptor';
import { GeneralService } from './service/general.service';
import { PdfViewerModule } from 'ng2-pdf-viewer';
import { ModalService } from './service/modal.service';
import { CarouselModule } from './components/carousel/carousel.module';
import { SwiperModule } from 'swiper/angular';
import { LoadingModule } from './components/loading/loading.module';
import { UserService } from './service/user.service';
import { CookieService } from 'ngx-cookie-service';
import { Subject } from 'rxjs';
import { TableService } from './service/table.service';
import { TableViewModule } from './components/tableview/table.module';
import { HeaderMenu } from './model/models';


const routes:Routes = [ 
  { path: '', loadChildren: () => import('./views/home/home.module').then(m => m.HomeModule)},
  { path: 'Login', loadChildren: () => import('./views/login/login.module').then(m => m.LoginModule)},
  { path: 'SignUp', loadChildren: () => import('./views/registration/registration.module').then(m => m.RegistrationModule)},
  { path: 'Home', loadChildren: () => import('./views/home/home.module').then(m => m.HomeModule)},
  { path: 'Reservation', loadChildren: () => import('./views/reservation/reservation.module').then(m => m.ReservationModule)},
  { path: 'Collaborator', loadChildren: () => import('./views/collaborator/collaborator.module').then(m => m.CollaboratorModule)},
  { path: 'Payments', loadChildren: () => import('./views/payments/payments.module').then(m => m.PaymentsModule)},
  { path: 'Table', loadChildren: () => import('./views/table/table.module').then(m => m.TableModule)},
  { path: 'Product', loadChildren: () => import('./views/product/product.module').then(m => m.ProductModule)},
  { path: 'TableOrder', loadChildren: () => import('./views/table_order/table_order.module').then(m => m.TableOrderModule)},
  { path: 'Events', loadChildren: () => import('./views/events/events.module').then(m => m.EventsModule)},
  { path: 'Events/Details', loadChildren: () => import('./views/events/event-detail/event-detail.module').then(m => m.EventDetailModule)},
  { path: 'MyProfile', loadChildren: () => import('./views/profile/profile.module').then(m => m.ProfileModule)},
  { path: 'HomeSettings', loadChildren: () => import('./views/home-settings/home-settings.module').then(m => m.HomeSettingsModule)},
  { path: 'Warehouse', loadChildren: () => import('./views/warehouse/warehouse.module').then(m => m.WarehouseModule)},
  { path: 'Coupon', loadChildren: () => import('./views/coupon/coupon.module').then(m => m.CouponModule)},
  { path: 'PhoneNumber', loadChildren: () => import('./views/coupon/coupon.module').then(m => m.CouponModule)},
  { path: 'Shop', loadChildren: () => import('./views/shop/shop.module').then(m => m.ShopModule)}
]
export const server_URL = "http://managedisco-001-site3.dtempurl.com/api/";

export const LOCALSTORARE_LOGIN_HEADER = "loginHeader";
export const LOCALSTORARE_LOGIN_HEADER_ENABLE_MENU = "loginHeaderEnableMenu";
export const LOCALSTORARE_LAST_PAGE_REQUEST = "lastPageRequest";
export const onLoginResponse: Subject<string> = new Subject<string>();
export const onMenuChange: Subject<boolean> = new Subject<boolean>();
export const maxTablePageRows = 10;

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    FormsModule,
    BrowserModule,
    HttpClientModule,
    ReactiveFormsModule,
    CarouselModule,
    LoadingModule,
    SwiperModule,
    PdfViewerModule,
    TableViewModule,
    RouterModule.forRoot(routes)
  ],
  providers: [
    ApiCaller,
    Endpoints,
    ApiHttpService,
    CookieService,
    GeneralService,
    UserService,
    TableService,
    ModalService,
    { provide: "urlRedirect", useValue:"/Login"},
    { provide: HTTP_INTERCEPTORS, useClass: ApiInterceptor, multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
