import React, { Component } from 'react';

class DashboardSideBar extends Component {
  render() {
    const navigationItems = pages => {
      // Highlight the selected page.
      return pages.map((page, index) => {
        const className = page.href === location.pathname ? 'active' : '';
        return <li key={index} className={className}><a href={page.href}>{page.name}</a></li>;
      });
    };

    // The user supplies pages in an array of sections.
    // Sections are an array of objects with a "name" and "href".
    const sections = this.props.pages.map((pages, index) => {
      return (
        <ul key={index} className="nav nav-sidebar">
          {navigationItems(pages)}
        </ul>
      );
    });

    return (
      <div className="row">
        <div className="col-sm-3 col-md-2 sidebar">
          {sections}
        </div>
      </div>
    );
  }
}

export default DashboardSideBar;
