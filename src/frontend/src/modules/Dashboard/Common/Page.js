import React, { Component } from 'react';
import NavigationBar from '../Components/Common/NavigationBar';
import SideBar from '../Components/Common/SideBar';

const pages = [
  [
    { name: 'Overview',     href: '/dashboard/home'         }
  ],
  [
    { name: 'Scrape Posts', href: '/dashboard/posts/scrape' },
    { name: 'Posts',        href: '/dashboard/posts'        }
  ],
  [
    { name: 'Scrape Pages', href: '/dashboard/pages/scrape' },
    { name: 'Pages',        href: '/dashboard/pages'
    }
  ]
];

class Page extends Component {
  render() {
    return (
      <div>
        <NavigationBar />
        <div className="container-fluid">
          <SideBar pages={pages} />
          <div className="col-sm-9 col-sm-offset-3 col-md-10 col-md-offset-2 main">
            <div className="row">
              {this.props.children}
            </div>
          </div>
        </div>
      </div>
    );
  }
}

export default Page;
