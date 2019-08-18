import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChannelboardComponent } from './channelboard.component';

describe('ChannelboardComponent', () => {
  let component: ChannelboardComponent;
  let fixture: ComponentFixture<ChannelboardComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChannelboardComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChannelboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
