import React, { Component } from 'react';
import { getPageScrapes, exportPages, getPages, scrapePages } from '../Common/Data/Actions';
import Panel from '../Common/Panel';
import ExportPages from '../Components/ScrapePages/ExportPages';
import PageScrapeList from '../Components/ScrapePages/PageScrapeList';
import ScrapePageForm from '../Components/ScrapePages/ScrapePageForm';

class ScrapePages extends Component {
  // Load the up-to-date scrape history each time the page is refreshed or loaded.
  componentWillMount () {
    this.getScrapes();
    this.context.store.dispatch(getPages());
  }

  getScrapes = (newPageNumber, newPageSize, since, until) => {
    const { pageNumber, pageSize } = this.context.store.getState().pageScrapes;
    this.context.store.dispatch(getPageScrapes(newPageNumber || pageNumber, newPageSize || pageSize, since, until));
  }

  handleExportToCSV = (since, until) => exportPages(since, until, (_, errorMessage) => {});
  
  handlePageScrapeClicked = (data, index) => window.location.href += '/' + data.id;

  handleScrapePages = (pages) => this.context.store.dispatch(scrapePages(pages));

  render() {
    const { pages, pageScrapes, errorMessage } = this.context.store.getState();

    return (
      <section>
        <ScrapePageForm pages={pages} errorMessage={errorMessage} onSubmit={this.handleScrapePages} />
        <section className="col-md-8">
          <ExportPages onSubmit={(since, until) => this.getScrapes(null, null, since, until)} onExport={this.handleExportToCSV} />
          <Panel showHeading={false} table={true}>
            <PageScrapeList scrapes={pageScrapes} errorMessage={errorMessage} onRowSelected={this.handlePageScrapeClicked} />
          </Panel>
        </section>
      </section>
    );
  }
}
ScrapePages.contextTypes = {store: React.PropTypes.object};

export default ScrapePages;
