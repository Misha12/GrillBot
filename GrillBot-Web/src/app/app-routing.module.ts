import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ChannelboardComponent } from './channelboard/channelboard.component';
import { HomeComponent } from './home/home.component';

const routes: Routes = [
  {
    path: 'channelboard',
    component: ChannelboardComponent
  },
  {
    path: '**',
    component: HomeComponent
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
